using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public partial struct SpawnResourcesSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) { }

    public void OnUpdate(ref SystemState state)
    {
        var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var list = World.DefaultGameObjectInjectionWorld.GetExistingSystem(typeof(BulletTriggersOnAsteroidsSystem));
        //list.
       // var triggers = world.GetExistingSystem<BulletTriggersOnAsteroidsSystem>().resourcesArray;
    }
}
