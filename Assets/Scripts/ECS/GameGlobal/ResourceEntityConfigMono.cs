using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;

public class ResourceEntityConfigMono : MonoBehaviour
{
    public GameObject [] resourcePrefab;
}


public class ResourceEntityConfigMonoBaker : Baker<ResourceEntityConfigMono>
{
    public override void Bake(ResourceEntityConfigMono authoring)
    {
        var resourceControllerEntity = GetEntity(TransformUsageFlags.None);
        
        if(authoring.resourcePrefab.Length > 0)
        {
            AddComponent(resourceControllerEntity, new ResourceSpawnerData
            {
                resource1 = Entity.Null // DynamicArray???
            });

           // var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var dynamicBuffer = AddBuffer<EntityElement>(resourceControllerEntity);

            for (int i = 0; i < authoring.resourcePrefab.Length; i++)
            {
                var convertedResource = GetEntity(authoring.resourcePrefab[i], TransformUsageFlags.Dynamic);
                dynamicBuffer.Add(new EntityElement { resource = convertedResource });
            }
        }
        
    }
}