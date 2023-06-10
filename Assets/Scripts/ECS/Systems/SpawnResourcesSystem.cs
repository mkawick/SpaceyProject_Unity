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
    //float currentTime = 0;
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
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

        var spawningEvents = btoa.resourceGenerationArray;
        int len = spawningEvents.Length;

        //var em = EntityManager;
        if (len > 0)
        {
            var entityManger = World.DefaultGameObjectInjectionWorld.EntityManager;
            var bufferFromEntity = GetBufferLookup<EntityElement>();

            Entities
               .WithAll<ResourceSpawnerData>()
               .ForEach((Entity entity) =>
               {
                   if (entityManger.HasBuffer<EntityElement>(entity) == false)
                       return;

                   var bufferFromEntity = entityManger.GetBuffer<EntityElement>(entity, true);

                   for (int i=0; i<spawningEvents.Length; i++)
                   {
                       var val = spawningEvents[i];
                       int index = val.resourceGeneratedTypeId;
                       if (val.wasProcessed == false && val.processCount < 2)// make sure that we only generate 1 resource per hit
                       {
                           //Debug.Log("spawning: " + val);
                           var instance = ecb.Instantiate(bufferFromEntity[index].resource);// potential to change the resource type here
                           var position = val.position;
                           ecb.SetComponent(instance, new LocalTransform { Position = position.Position, Scale = 1, Rotation = Quaternion.identity });

                           float3 dir = new float3(0, 0, 1);
                           ecb.AddComponent<MoveControllerData>(instance, new MoveControllerData { direction = -dir, speed = 5, turnSpeed = 0.0f, accelerationSpeed = -1 });
                           val.wasProcessed = true;
                           spawningEvents[i] = val;// push back
                       }
                   }
               }).Run();
        }

        Dependency.Complete();
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}
