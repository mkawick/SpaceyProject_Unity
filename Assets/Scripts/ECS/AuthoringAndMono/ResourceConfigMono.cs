using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public class ResourceConfigMono : MonoBehaviour
{
    public GameObject [] resourcePrefab;
}


public class ResourceConfigMonoBaker : Baker<ResourceConfigMono>
{
    public override void Bake(ResourceConfigMono authoring)
    {
        var attackControllerEntity = GetEntity(TransformUsageFlags.Dynamic);
        
        if(authoring.resourcePrefab.Length > 0)
        {
            if (authoring.resourcePrefab[0] != null)
            {
                var convertedResource = GetEntity(authoring.resourcePrefab[0], TransformUsageFlags.Dynamic);
                AddComponent(attackControllerEntity, new ResourceSpawnerData
                {
                    resource1 = convertedResource
                });
            }
        }
        
    }
}