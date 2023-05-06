using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using Unity.Physics;

public partial class SpawnResourcesSystem : SystemBase
{
    float currentTime = 0;
   // [BurstCompile]    
    protected override void OnCreate()
    {
    }

    protected override void OnUpdate()
    {
        var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var systemHandle = World.GetExistingSystem<BulletTriggersOnAsteroidsSystem>();
        var type = systemHandle.GetType();

        BulletTriggersOnAsteroidsSystem btoa = World.Unmanaged.GetUnsafeSystemRef<BulletTriggersOnAsteroidsSystem>(systemHandle);

        //BulletTriggersOnAsteroidsSystem btoa = WorldUnmanaged.GetUnsafeSystemRef<BulletTriggersOnAsteroidsSystem>(systemHandle);


       // BulletTriggersOnAsteroidsSystem btoa = WorldUnmanaged.ResolveSystem<BulletTriggersOnAsteroidsSystem>(systemHandle);

        //SystemHandle handle = World.DefaultGameObjectInjectionWorld.GetExistingSystem(typeof(ProjectIntoFutureOnCueSystem));
        //var t = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentObject<BulletTriggersOnAsteroidsSystem>(systemHandle);
        //BulletTriggersOnAsteroidsSystem sys = EntityManager.<BulletTriggersOnAsteroidsSystem>(systemHandle);

        //var data = EntityManager.GetComponentData<NativeList<AsteroidHitList>>(SystemHandle);
        var spawningEvents = btoa.targetsArray;

        //var t = World.GetOrCreateSystemManaged<BulletTriggersOnAsteroidsSystem>();
        //WorldUnmanaged.GetUnsafeSystemRef<BulletTriggersOnAsteroidsSystem>(systemHandle);
        //var listOfImpacts = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BulletTriggersOnAsteroidsSystem>(); 

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        int len = spawningEvents.Length;
        if (len > 0)
        {
            Entities
                .ForEach((ref ResourceSpawnerData resources) =>
                {
                    foreach (var val in spawningEvents)
                    {
                        Debug.Log(val.processCount);
                    }

                }).Schedule();
        }

       // Dependency.Complete();
        ecb.Playback(EntityManager);

        // You are responsible for disposing of any ECB you create.
        ecb.Dispose();
    }
}
