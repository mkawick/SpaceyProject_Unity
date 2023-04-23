using UnityEngine;
using Unity.Entities;

public class ProjectileTagMono : MonoBehaviour
{
   // public float tag;
}


public class ProjectileTagBaker : Baker<ProjectileTagMono>
{
    public override void Bake(ProjectileTagMono authoring)
    {
        var projectileTagEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(projectileTagEntity, new ProjectileTag
        {
        }) ;

    }
}