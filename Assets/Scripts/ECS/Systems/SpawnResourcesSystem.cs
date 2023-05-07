using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Unity.Physics;

//[BurstCompile]
public partial class SpawnResourcesSystem : SystemBase
{
    float currentTime = 0;
 //   [BurstCompile]    
    protected override void OnCreate()
    {
    }

  //  [BurstCompile]
    protected override void OnUpdate()
    {
        var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var systemHandle = World.GetExistingSystem<BulletTriggersOnAsteroidsSystem>();
        var type = systemHandle.GetType();

        BulletTriggersOnAsteroidsSystem btoa = World.Unmanaged.GetUnsafeSystemRef<BulletTriggersOnAsteroidsSystem>(systemHandle);
        var spawningEvents = btoa.resourceGenerationArray;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        int len = spawningEvents.Length;
        if (len > 0)
        {
            Entities
                .ForEach((ref ResourceSpawnerData resources) =>
                {
                    foreach (var val in spawningEvents)
                    {
                        //Debug.Log(val.processCount);
                        int index = val.resourceGeneratedTypeId;
                        if (val.processCount < 2)
                        {
                            var resourceEntityToGenerate = resources.resource1;// [resources];

                            var instance = ecb.Instantiate(resourceEntityToGenerate);// potential to change the resource type here
                            var position = val.position;
                            ecb.SetComponent(instance, new LocalTransform { Position = position.Position, Scale = 1, Rotation = Quaternion.identity });

                            float3 dir = new float3(0, 0, 1);
                            ecb.AddComponent<MoveControllerData>(instance, new MoveControllerData { direction = dir, speed = 5, turnSpeed = 0.0f });
                        }
                    }

                }).Schedule();
        }

        Dependency.Complete();
        ecb.Playback(EntityManager);

        // You are responsible for disposing of any ECB you create.
        ecb.Dispose();
    }
}
