using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NumericsConversion;
using System;
using System.Threading.Tasks;
using Microsoft.MixedReality.SceneUnderstanding;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.OpenXR;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WindowsMR;
#if ENABLE_WINMD_SUPPORT
using Windows.Perception.Spatial;
using Windows.Perception.Spatial.Preview;
#endif

public class PlaceHoop : MonoBehaviour
{

    Scene world;
    public GameObject hoop;
    public GameObject marker;
    public GameObject root;
    public GameObject hoopButton;
    public GameObject basketball;
    public Material quadMaterial;
    public Log log;
    ArrayList quads = new ArrayList();
    List<GameObject> buttons = new List<GameObject>();
    bool complete = false;
    bool cooldown = false;
    bool validated = false;

    async void Start()
    {
        // Check if scene observation is supported
        if (!SceneObserver.IsSupported())
        {
            log.write("OBJECT 'SceneObserver' NOT SUPPORTED IN CURRENT CONTEXT");
            return;
        }

        // Request access to the SceneObserver
        var check = await SceneObserver.RequestAccessAsync();
        if(check == SceneObserverAccessStatus.Allowed)
        {
            validated = true;
            log.write("Permission granted");
        } else
        {
            log.write("ACCESS DENIED");
            Destroy(this);
        }

        // Initial "cooldown" to allow time to scan the scene
        cooldown = true;
        StartCoroutine("RunCooldown");
    }

    async private void Update()
    {
        // If 1) We have access to the SceneObserver, 2) A scene hasn't already been created, and 3) The cooldown isn't running, attempt to create a new scene.
        if(validated && !complete && !cooldown)
        {
            await ValidateScene();
        }

        // Updates the world coordinates if a scene has been created
        if (world != null)
        {
#if ENABLE_WINMD_SUPPORT
            var node = world.OriginSpatialGraphNodeId;

            var sceneCoordSystem = SpatialGraphInteropPreview.CreateCoordinateSystemForNode(node);

            var unityCoordSystem = PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;

            var transform = sceneCoordSystem.TryGetTransformTo(unityCoordSystem);

            if (transform.HasValue)
            {
                var sceneToWorldUnity = transform.Value.ToUnity();

                root.transform.SetPositionAndRotation(
                    sceneToWorldUnity.GetColumn(3), sceneToWorldUnity.rotation);
            }
#endif
        }
    }

    private void FixedUpdate()
    {
        // Check if a button has been pressed
        if(buttons.Count != 0)
        {
            foreach (GameObject button in buttons)
            {
                if(button.GetComponent<ButtonScript>().isPressed())
                {
                    buttonPressed(button);
                }
            }
        }

    }


    async Task CreateScene()
    {
        // Create the settings for scene generation
        SceneQuerySettings settings;

        settings.EnableSceneObjectQuads = true;                                       // Requests that the scene updates quads.
        settings.EnableSceneObjectMeshes = true;                                      // Requests that the scene updates watertight mesh data.
        settings.EnableOnlyObservedSceneObjects = false;                              // Do not explicitly turn off quad inference.
        settings.EnableWorldMesh = true;                                              // Requests a static version of the spatial mapping mesh.
        settings.RequestedMeshLevelOfDetail = SceneMeshLevelOfDetail.Fine;            // Requests the finest LOD of the static spatial mapping mesh.

        // Initialize a new Scene
        world = await SceneObserver.ComputeAsync(settings, 10.0f);
        log.write("SCENE CREATED!\n" + world.ToString());
    }

    bool SpawnHoop(SceneObject wall)
    {
        if(wall.Quads.Count > 0)
        {
            log.write("Wall has quads!");
            GameObject m = Instantiate(marker);
            m.transform.SetParent(root.transform);
            m.transform.localPosition = ((System.Numerics.Vector3) wall.Position).ToUnity();
            m.transform.localRotation = ((System.Numerics.Quaternion) wall.Orientation).ToUnity();

            if(!GetComponent<DisableMesh>().disabled)
            {
                GetComponent<DisableMesh>().Disable();
            }


            foreach (var sceneQuad in wall.Quads)
            {
                /**
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Cube);

                quad.transform.SetParent(m.transform, false);

                quad.transform.localScale = new Vector3(
                    sceneQuad.Extents.X, sceneQuad.Extents.Y, 0.025f);

                quad.GetComponent<Renderer>().material = quadMaterial;
                quad.layer = 8;
                quads.Add(quad);
                **/

                GameObject button = Instantiate(hoopButton);
                button.transform.position = m.transform.position;
                button.transform.rotation = m.transform.rotation;
                button.transform.SetParent(m.transform, true);
                button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y, button.transform.localPosition.z - 0.025f);
                button.SetActive(true);
                buttons.Add(button);

                log.write("Made a new button!");
            }

            //GameObject h = Instantiate(hoop);
            //h.transform.SetParent(m.transform, false);


            return true;
        }
        else
        {
            log.write("No Quads in wall!");
            return false;
        }
    }

    bool CreateHoop()
    {
        log.write("Creating Hoop...");
        bool created = false;
        foreach(SceneObject obj in world.SceneObjects)
        {
            if(obj.Kind == SceneObjectKind.Wall)
            {
                log.write("FOUND A WALL!!!");
                bool temp = SpawnHoop(obj);
                if(!created) 
                    created = temp;
            } else
            {
                log.write("FOUND A " + obj.Kind);
            }
        }
        if(world.SceneObjects.Count == 0)
        {
            log.write("No objects in scene :(");
        }

        return created;
    }

    async Task ValidateScene()
    {
        cooldown = true;
        await CreateScene();
        bool created = CreateHoop();
        if(!created)
        {
            log.write("Hoop could not be spawned. Trying again...");
            StartCoroutine("RunCooldown");
        } else
        {
            log.write("HOOP SPAWNED");
            complete = true;
        }
    }

    IEnumerator RunCooldown()
    {
        yield return new WaitForSeconds(15);
        cooldown = false;
    }

    public void buttonPressed(GameObject button)
    {
        log.write("Button Pressed!");
        Transform wall = button.transform.parent;
        GameObject h = Instantiate(hoop);
        h.transform.position = wall.position;
        h.transform.rotation = wall.rotation;
        h.transform.SetParent(wall, true);

        GameObject[] ButtonArray = buttons.ToArray();
        buttons.Clear();
        for(int i = 0; i < ButtonArray.Length; i++)
        {
            GameObject temp = ButtonArray[i];
            ButtonArray[i] = null;
            Destroy(temp);
        }

        GameObject ball = Instantiate(basketball);
        ball.layer = 7;
        ball.transform.position = Vector3.zero;

        // Stop spatial observation
        Microsoft.MixedReality.Toolkit.CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySpatialAwarenessMeshObserver>().Suspend();
    }
}
