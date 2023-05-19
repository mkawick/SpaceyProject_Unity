using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;

public partial class ResourceGatheringSystem : SystemBase
{
    public Action<ResourceType, float, int> OnCollect;

    struct Notification
    {
        public ResourceType resourceType;
        public float quantity;
        public int playerId;
    }
    protected override void OnUpdate()
    {
        var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var systemHandle = World.GetExistingSystem<BulletTriggersOnAsteroidsSystem>();
        float deltaTime = SystemAPI.Time.DeltaTime;

        BulletTriggersOnAsteroidsSystem btoa = World.Unmanaged.GetUnsafeSystemRef<BulletTriggersOnAsteroidsSystem>(systemHandle);
        var resourceCollectEvents = btoa.resourceGatheredArray;
        if (resourceCollectEvents.Length == 0)
            return;

        List<Notification> notificationList = new List<Notification>();

        Entities
             .WithoutBurst()
             .ForEach((Entity entity, in PlayerTag player) =>             
             {
                 for (int i = 0; i < resourceCollectEvents.Length; i++)
                 {
                     var collect = resourceCollectEvents[i];
                     if (collect.playerId == player.id)
                     {
                         collect.wasProcessed = true;
                         resourceCollectEvents[i] = collect;
                         notificationList.Add(new Notification
                         {
                             resourceType = collect.type,
                             quantity = collect.quantity,
                             playerId = collect.playerId
                         });
                     }
                 }
             }).Run();

        foreach(Notification notification in notificationList)
        {
            OnCollect?.Invoke(notification.resourceType, notification.quantity, notification.playerId);
        }
        
    }

}
