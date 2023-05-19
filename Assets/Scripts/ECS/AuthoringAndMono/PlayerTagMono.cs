using UnityEngine;
using Unity.Entities;


public class PlayerTagMono : MonoBehaviour
{
    // consider things like layes, filters, rate of collection
    /* public float percentDrop;
     public ResourceType resourceType1; // resources and percentages
     public float percentToDrop1;*/
    public int id;
}


public class PlayerTagBaker : Baker<PlayerTagMono>
{
    public override void Bake(PlayerTagMono authoring)
    {
        var collectorTagEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(collectorTagEntity, new PlayerTag
        {
           id = authoring.id
        });

    }
}