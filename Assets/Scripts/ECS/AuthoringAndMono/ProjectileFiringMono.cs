using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public class ProjectileFiringMono : MonoBehaviour
{
    public GameObject projectile;
    public float spawnRate;
    
    //public Transform projectileSpawn;
}


public class ProjectileFiringBaker : Baker<ProjectileFiringMono>
{
    public override void Bake(ProjectileFiringMono authoring)
    {
        var projectileFiringEntity = GetEntity(TransformUsageFlags.Dynamic);
        var convertedProjectile = GetEntity(authoring.projectile, TransformUsageFlags.Dynamic);
        float scale = authoring.projectile.transform.localScale.x;

        AddComponent(projectileFiringEntity, new ProjectileFiringData
        {
            spawnRate = authoring.spawnRate,
            projectile = convertedProjectile,
            scale = scale
            //projectileSpawn = Transform.Instantiate(authoring.projectileSpawn.gameObject, authoring.projectileSpawn.position, authoring.projectileSpawn.rotation)
        }); ;

    }
}

// for taking damage, we may need this:
// AddBuffer<BrainDamageBufferElement>(brainEntity);