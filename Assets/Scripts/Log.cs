using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.IO;
#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
#endif

public class Log : MonoBehaviour
{
    string filepath = "Assets/Scene-Output";
    string filename = "scene-output.txt";
    string folder = "Scene-Output";
    Queue q = new Queue();

    public async void Start()
    {
        filename = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + "-" + filename;
        await startLog();
        write("STARTING");
        StartCoroutine("WriteAll");
    }

    async Task startLog()
    {
#if ENABLE_WINMD_SUPPORT
        StorageFolder sessionParentFolder = await KnownFolders.PicturesLibrary
                .CreateFolderAsync(folder,
                CreationCollisionOption.OpenIfExists);
        filepath = sessionParentFolder.Path;
#endif
    }

    public void write(string s)
    {
        Debug.Log(s);
        q.Enqueue(s);
    }

    private IEnumerator WriteAll()
    {
        while (true)
        {
            string path = Path.Combine(filepath, filename);
            StreamWriter sw = null;
            while (q.Count > 0)
            {
                if (sw == null) sw = new StreamWriter(path, true);
                sw.WriteLine(q.Dequeue());
            }
            if (sw != null) sw.Close();

            yield return new WaitForSeconds(2);
        }
    }
}
