using UnityEngine;
using Unity.Entities;


public class CollectorTagMono : MonoBehaviour
{
    // consider things like layes, filters, rate of collection
   /* public float percentDrop;
    public ResourceType resourceType1; // resources and percentages
    public float percentToDrop1;*/
}


public class CollectorTagBaker : Baker<CollectorTagMono>
{
    public override void Bake(CollectorTagMono authoring)
    {
        var collectorTagEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(collectorTagEntity, new CollectorTag
        {
           /* percentDrop = authoring.percentDrop,
            resourceType1 = (int)authoring.resourceType1,
            percentToDrop1 = authoring.percentToDrop1,*/
        });

    }
}