using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public class ResourceEntityConfigMono : MonoBehaviour
{
    public GameObject [] resourcePrefab;
}


public class ResourceEntityConfigMonoBaker : Baker<ResourceEntityConfigMono>
{
    public override void Bake(ResourceEntityConfigMono authoring)
    {
        var attackControllerEntity = GetEntity(TransformUsageFlags.Dynamic);
        
        if(authoring.resourcePrefab.Length > 0)
        {
            if (authoring.resourcePrefab[0] != null)
            {
                var convertedResource = GetEntity(authoring.resourcePrefab[0], TransformUsageFlags.Dynamic);
                AddComponent(attackControllerEntity, new ResourceSpawnerData
                {
                    resource1 = convertedResource // DynamicArray???
                });
            }
        }
        
    }
}