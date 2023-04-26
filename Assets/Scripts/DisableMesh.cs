using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;

public class DisableMesh : MonoBehaviour
{
    public bool disabled = false;

    public void Disable()
    {
        if (MixedRealityServiceRegistry.TryGetService<IMixedRealitySpatialAwarenessSystem>(out var service))
        {
            IMixedRealityDataProviderAccess dataProviderAccess = service as IMixedRealityDataProviderAccess;
            IMixedRealitySpatialAwarenessMeshObserver observer = dataProviderAccess.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();
            observer.DisplayOption = SpatialAwarenessMeshDisplayOptions.None;
            disabled = true;
        }
    }
}
