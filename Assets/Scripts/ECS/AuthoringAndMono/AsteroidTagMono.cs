using UnityEngine;
using Unity.Entities;

public enum ResourceType
{
    Aluminum, 
    Copper, 
    Iron, 
    Izithrium
}

public class AsteroidTagMono : MonoBehaviour
{
    public float percentDrop;
    public ResourceType resourceType1; // resources and percentages
    public float percentToDrop1;
}


public class AsteroidTagBaker : Baker<AsteroidTagMono>
{
    public override void Bake(AsteroidTagMono authoring)
    {
        var projectileTagEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(projectileTagEntity, new AsteroidTag
        {
            percentDrop = authoring.percentDrop,
            resourceType1 = (int) authoring.resourceType1,
            percentToDrop1 = authoring.percentToDrop1,
        });

    }
}