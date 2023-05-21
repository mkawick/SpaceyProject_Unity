using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

[RequireMatchingQueriesForUpdate]
public partial class ProjectileFiringSystem : SystemBase
{
    float currentTime = 0;
    protected override void OnCreate()
    {
    }
    protected override void OnUpdate()
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        Entities
            .ForEach((ref ProjectileFiringData projectileData, in AttackControllerData acd, in PlayerTag player, in Entity e,  in LocalTransform transform) =>
            {
                if (projectileData.projectile == null)
                    return;
                if (acd.isFiring == true)
                {
                    if (projectileData.spawnRate < projectileData.currentTime)
                    {
                        projectileData.currentTime = 0;
                        var instance = ecb.Instantiate(projectileData.projectile);

                        //float scale = 0.2;//TODO, convert to a transform pass thru at baking time

                        float3 shipRotation = transform.Forward();    // no sure why but the ship is pointing the wrong way... should be transform.Forward();

                        var newPos = transform.Position + shipRotation * 2;
                        ecb.SetComponent(instance, new LocalTransform { Position = newPos, Scale = projectileData.scale, Rotation = transform.Rotation });
                        ecb.SetComponent(instance, new ProjectileTag { playerId = player.id });

                        ecb.AddComponent<MoveControllerData>(instance, new MoveControllerData { direction = shipRotation, speed = 5, turnSpeed = 0.0f });
                    }
                }
                projectileData.currentTime += deltaTime;
            }).Schedule();

        Dependency.Complete();
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
    
}
