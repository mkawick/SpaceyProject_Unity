using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public class ResourceMono : MonoBehaviour
{
    public ResourceType type;
    public int quantity;
}

public class ResourceMonoBaker : Baker<ResourceMono>
{
    public override void Bake(ResourceMono authoring)
    {
        var resourceEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(resourceEntity, new ResourceData
        {
            type = authoring.type,
            quantity = authoring.quantity
        });

    }
}