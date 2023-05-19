using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using System.Linq;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
/*
 // video https://www.youtube.com/watch?v=lkR6i6aTvhY
 // code https://github.com/WAYN-Games/DOTS-Training/blob/DOTS-112/Assets/Scripts/Systems/ProjectileSystem.cs

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
[BurstCompile]
public partial struct ProjectileSystem : ISystem
{
    ComponentLookup<LocalTransform> positionLookup;
    ComponentLookup<Impact> impactLookup;
    BufferLookup<HitList> hitListLookup;
    ComponentLookup<Health> healthLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        positionLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
        impactLookup = SystemAPI.GetComponentLookup<Impact>(false);
        hitListLookup = SystemAPI.GetBufferLookup<HitList>();
        healthLookup = SystemAPI.GetComponentLookup<Health>(false);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbBOS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        foreach (var (towerData, transform) in SystemAPI.Query<RefRW<TowerData>, TransformAspect>())
        {
            towerData.ValueRW.TimeToNextSpawn -= SystemAPI.Time.DeltaTime;
            if (towerData.ValueRO.TimeToNextSpawn < 0)
            {
                ClosestHitCollector<DistanceHit> closestHitCollector = new ClosestHitCollector<DistanceHit>(towerData.ValueRO.Range);
                if (physicsWorld.OverlapSphereCustom(transform.WorldPosition, towerData.ValueRO.Range, ref closestHitCollector, towerData.ValueRO.Filter))
                {
                    towerData.ValueRW.TimeToNextSpawn = towerData.ValueRO.Timer;
                    Entity e = ecbBOS.Instantiate(towerData.ValueRO.Prefab);
                    ecbBOS.SetComponent(e,
                        LocalTransform.FromMatrix(
                            float4x4.LookAt(transform.WorldPosition,
                            closestHitCollector.ClosestHit.Position,
                            transform.Up)));
                    ecbBOS.AddComponent(e, new Target() { Value = closestHitCollector.ClosestHit.Entity });

                }

            }
        }

        positionLookup.Update(ref state);

        foreach (var (speed, target, transform, entity) in SystemAPI.Query<RefRO<Speed>, RefRO<Target>, TransformAspect>().WithEntityAccess())
        {
            if (positionLookup.HasComponent(target.ValueRO.Value))
            {
                if (!SystemAPI.HasBuffer<HitList>(entity))
                    transform.LookAt(positionLookup[target.ValueRO.Value].Position);

                transform.WorldPosition = transform.WorldPosition + speed.ValueRO.value * SystemAPI.Time.DeltaTime * transform.Forward;
            }
            else
            {
                ecbBOS.DestroyEntity(entity);
            }
        }

        positionLookup.Update(ref state);

        healthLookup.Update(ref state);

        foreach (var (target, transform, impact, entity) in SystemAPI.Query<RefRO<Target>, TransformAspect, RefRO<Impact>>().WithEntityAccess().WithNone<HitList>())
        {
            if (positionLookup.HasComponent(target.ValueRO.Value))
            {
                if (math.distance(positionLookup[target.ValueRO.Value].Position, transform.WorldPosition) < 0.1f)
                {
                    Health hp = healthLookup[target.ValueRO.Value];
                    hp.Value -= 5;
                    healthLookup[target.ValueRO.Value] = hp;

                    Entity impactEntity = ecbBOS.Instantiate(impact.ValueRO.Prefab);
                    ecbBOS.SetComponent(impactEntity,
                        LocalTransform.FromPosition(positionLookup[target.ValueRO.Value].Position));

                    if (hp.Value < 0)
                    {
                        ecbBOS.DestroyEntity(target.ValueRO.Value);
                    }
                    ecbBOS.DestroyEntity(entity);
                }
            }
        }

        SimulationSingleton simulation = SystemAPI.GetSingleton<SimulationSingleton>();

        positionLookup.Update(ref state);
        healthLookup.Update(ref state);
        impactLookup.Update(ref state);
        hitListLookup.Update(ref state);

        state.Dependency = new ProjectileHitJob()
        {
            Projectiles = impactLookup,
            EnemiesHealth = healthLookup,
            Positions = positionLookup,
            HitLists = hitListLookup,
            ECB = ecbBOS
        }.Schedule(simulation, state.Dependency);


    }

    [BurstCompile]
    public struct ProjectileHitJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<LocalTransform> Positions;
        public ComponentLookup<Impact> Projectiles;
        public ComponentLookup<Health> EnemiesHealth;

        public EntityCommandBuffer ECB;
        public BufferLookup<HitList> HitLists;

        public void Execute(TriggerEvent triggerEvent)
        {


            Entity projectile = Entity.Null;
            Entity enemy = Entity.Null;

            // Identiy which entity is which
            if (Projectiles.HasComponent(triggerEvent.EntityA))
                projectile = triggerEvent.EntityA;
            if (Projectiles.HasComponent(triggerEvent.EntityB))
                projectile = triggerEvent.EntityB;
            if (EnemiesHealth.HasComponent(triggerEvent.EntityA))
                enemy = triggerEvent.EntityA;
            if (EnemiesHealth.HasComponent(triggerEvent.EntityB))
                enemy = triggerEvent.EntityB;

            // if its a pair of entity we don't want to process, exit
            if (Entity.Null.Equals(projectile)
                || Entity.Null.Equals(enemy)) return;


            // Check we did not already hit that traget in previous frames
            var hits = HitLists[projectile];
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].Entity.Equals(enemy))
                    return;
            }

            // Add enemy to list of already hit entities
            // to avoid hitting it next frame due to the
            // stateless nature of the Physics
            hits.Add(new HitList { Entity = enemy });

            // Damage enemy
            Health hp = EnemiesHealth[enemy];
            hp.Value -= 5;
            EnemiesHealth[enemy] = hp;

            // Destroy enemy if it is out of health
            if (hp.Value <= 0)
                ECB.DestroyEntity(enemy);

            // Spawn VFX
            Entity impactEntity = ECB.Instantiate(Projectiles[projectile].Prefab);
            ECB.SetComponent(impactEntity,
                LocalTransform.FromPosition(Positions[enemy].Position));

            // Destroy projectile if it hits all its targets
            if (Projectiles[projectile].MaxImpactCount <= HitLists[projectile].Length)
                ECB.DestroyEntity(projectile);

        }

    }
}*/

public struct AsteroidHitList : IBufferElementData
{
    public LocalTransform position;
    public int playerProjectileOwnerId;
    public int hitCount;
    public int resourceGeneratedTypeId;
    public Entity asteroidHit;
    public Entity projectileFired;
    public int processCount;
    public float currentTime;
    public bool wasProcessed;
}

public struct ResourceGathered : IBufferElementData
{
    public Entity resourceEntity;    
    public ResourceType type;
    public int playerId;
    public float quantity;
    public int processCount;
    public float currentTime;
    public bool wasProcessed;
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct BulletTriggersOnAsteroidsSystem : ISystem
{
    public NativeList<AsteroidHitList> resourceGenerationArray;
    public NativeList<ResourceGathered> resourceGatheredArray;
    ComponentLookup<ProjectileTag> projectileLookup;
    ComponentLookup<AsteroidTag> asteroidLookup;
    ComponentLookup<LocalTransform> positionLookup;
    ComponentLookup<PlayerTag> collectorLookup;
    ComponentLookup<ResourceData> resourceLookup;
    //BufferLookup<AsteroidHitList> hitListLookup; // we want different lists for asteroids, player combat, and enemy combat
    /*  ComponentLookup<Impact> impactLookup;

      ComponentLookup<Health> healthLookup;*/

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        resourceGenerationArray = new NativeList<AsteroidHitList>(10000, Allocator.Persistent);
        resourceGatheredArray = new NativeList<ResourceGathered>(10000, Allocator.Persistent);
        int num = resourceGenerationArray.Length;
    }

    void OnDestroy()
    {
        resourceGenerationArray.Dispose();
    }
    bool CleanupHistory(float currentTime, EntityCommandBuffer ecb)
    {
        for (int i = resourceGenerationArray.Length - 1; i >= 0; i--)
        {
            var item = resourceGenerationArray[i];
            if (item.wasProcessed == true || currentTime > item.currentTime + 0.2f) // 1/5th of a second
            {
                ecb.DestroyEntity(item.projectileFired);
                resourceGenerationArray.RemoveAt(i);
            }
        }

        for (int i = resourceGatheredArray.Length - 1; i >= 0; i--)
        {
            var item = resourceGatheredArray[i];
            if (item.wasProcessed == true || currentTime > item.currentTime + 0.2f) // 1/5th of a second
            {
                ecb.DestroyEntity(item.resourceEntity);
                resourceGatheredArray.RemoveAt(i);
            }
        }

        return true;
    }

    //  [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        projectileLookup = SystemAPI.GetComponentLookup<ProjectileTag>(false);// not read only
        asteroidLookup = SystemAPI.GetComponentLookup<AsteroidTag>(false);
        positionLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);// read only

        collectorLookup = SystemAPI.GetComponentLookup<PlayerTag>(true);
        resourceLookup = SystemAPI.GetComponentLookup<ResourceData>(true);

        float currentTime = SystemAPI.Time.fixedDeltaTime;
        var ecbBOS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        CleanupHistory(currentTime, ecbBOS);

        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        SimulationSingleton simulation = SystemAPI.GetSingleton<SimulationSingleton>();

        state.Dependency.Complete();
        var job = new ProjectileHitJob
        {
            ECB = ecbBOS,
            Projectiles = projectileLookup,
            Asteroids = asteroidLookup,
            Positions = positionLookup,
            Resources = resourceLookup,
            Collector = collectorLookup,
            resourceGenerationArray = resourceGenerationArray,
            resourceGatheredArray = resourceGatheredArray,
            currentTime = currentTime
        };
        var handle = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        handle.Complete();

        //ecbBOS.Playback(EntityManager);
        //ecbBOS.Dispose();
    }

    // [BurstCompile]
    public struct ProjectileHitJob : ITriggerEventsJob
    {
        public EntityCommandBuffer ECB;
        [ReadOnly] public ComponentLookup<LocalTransform> Positions;
        public ComponentLookup<ProjectileTag> Projectiles;
        public ComponentLookup<AsteroidTag> Asteroids;
        [ReadOnly] public ComponentLookup<ResourceData> Resources;
        [ReadOnly] public ComponentLookup<PlayerTag> Collector;
        public NativeList<AsteroidHitList> resourceGenerationArray;
        public NativeList<ResourceGathered> resourceGatheredArray;
        public float currentTime;
        

        public void Execute(TriggerEvent triggerEvent)
        {
            if (CollectorAndResourceTrigger(triggerEvent))
                return;

            if (AsteroidAndProjectileTrigger(triggerEvent))
                return;
        }

        public bool CollectorAndResourceTrigger(TriggerEvent triggerEvent)
        {
            Entity resource = Entity.Null;
            Entity collector = Entity.Null;

            if (Collector.HasComponent(triggerEvent.EntityA))
            {
                collector = triggerEvent.EntityA;
                if (Resources.HasComponent(triggerEvent.EntityB))
                    resource = triggerEvent.EntityB;
            }
            if (Collector.HasComponent(triggerEvent.EntityB))
            {
                collector = triggerEvent.EntityB;
                if (Resources.HasComponent(triggerEvent.EntityA))
                    resource = triggerEvent.EntityA;
            }

            if (collector == Entity.Null)
            {
                Debug.Log("collector null");
                return false;
            }
            if (resource == Entity.Null)
            {
                Debug.Log("resource null");
                return false;
            }

            Debug.Log("* collect *");
            Resources.TryGetComponent(resource, out ResourceData resourceData);
            Collector.TryGetComponent(collector, out PlayerTag playerTag);
            resourceGatheredArray.Add(new ResourceGathered
            {
                resourceEntity = resource, 
                type = resourceData.type,
                playerId = playerTag.id,
                quantity = resourceData.quantity,
                processCount = 0,
                currentTime = currentTime
            });

            return true;
        }

        bool AsteroidAndProjectileTrigger(TriggerEvent triggerEvent)
        {
            Entity projectile = Entity.Null;
            Entity asteroid = Entity.Null;

            if (Projectiles.HasComponent(triggerEvent.EntityA))
            {
                projectile = triggerEvent.EntityA;
                if (Asteroids.HasComponent(triggerEvent.EntityB))
                    asteroid = triggerEvent.EntityB;
            }
            if (Projectiles.HasComponent(triggerEvent.EntityB))
            {
                projectile = triggerEvent.EntityB;
                if (Asteroids.HasComponent(triggerEvent.EntityA))
                    asteroid = triggerEvent.EntityA;
            }

            if (projectile == Entity.Null)
            {
                return false;
            }
            if (asteroid == Entity.Null)
            {
                return false;
            }

            Debug.Log("mining");

            Projectiles.TryGetComponent(asteroid, out ProjectileTag projectileTag);
            Positions.TryGetComponent(asteroid, out LocalTransform projectilePosition);
            Asteroids.TryGetComponent(asteroid, out AsteroidTag asteroidTag);

            int length = resourceGenerationArray.Length;
            for (int i = 0; i < length; i++)
            {
                var resourceGen = resourceGenerationArray[i];
                if (projectile == resourceGen.projectileFired)
                {
                    resourceGen.processCount++;
                    resourceGenerationArray[i] = resourceGen;// assign back
                    return true;
                }
            }
            resourceGenerationArray.Add(new AsteroidHitList
            {
                asteroidHit = asteroid,
                projectileFired = projectile,
                hitCount = 1,
                playerProjectileOwnerId = projectileTag.playerId,
                resourceGeneratedTypeId = asteroidTag.resourceType1,
                position = projectilePosition,
                processCount = 0,
                currentTime = currentTime
            });
            return true;
        }

    }
}


