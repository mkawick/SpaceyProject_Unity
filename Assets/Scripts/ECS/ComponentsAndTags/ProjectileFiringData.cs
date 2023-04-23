using Unity.Entities;
using Unity.Mathematics;

public struct ProjectileFiringData : IComponentData
{
    public Entity projectile;   // prefab for projectile
    //public LocalTransform projectileSpawn;  // transform of the spawn point, may need to reference the actual entity
    public float spawnRate;
    public float currentTime;
    public float scale; // from the tarnsform
}
