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
    public int processCount;
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
public partial struct BulletTriggersOnAsteroidsSystem : ISystem
{
    NativeList<AsteroidHitList> targetsArray;
    ComponentLookup<ProjectileTag> projectileLookup;
    ComponentLookup<AsteroidTag> asteroidLookup;
    ComponentLookup<LocalTransform> positionLookup;
    //BufferLookup<AsteroidHitList> hitListLookup; // we want different lists for asteroids, player combat, and enemy combat
    /*  ComponentLookup<Impact> impactLookup;

      ComponentLookup<Health> healthLookup;*/

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        targetsArray = new NativeList<AsteroidHitList>(10000, Allocator.Persistent);
        //Length(targetsArray);
        int num = targetsArray.Length;
        
                                                                            // hitListLookup = SystemAPI.GetBufferLookup<AsteroidHitList>();// needs to be created???

        //
    }

    void OnDestroy()
    {
        targetsArray.Dispose();
    }
    //  [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        projectileLookup = SystemAPI.GetComponentLookup<ProjectileTag>(false);// not read only
        asteroidLookup = SystemAPI.GetComponentLookup<AsteroidTag>(false);
        positionLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);// read only
        //return;

        //Debug.Log("BulletTriggersOnAsteroidsSystem.Update");
        var ecbBOS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        SimulationSingleton simulation = SystemAPI.GetSingleton<SimulationSingleton>();

        state.Dependency.Complete();
        var job = new ProjectileHitJob
        {
            Projectiles = projectileLookup,
            Asteroids = asteroidLookup,
            Positions = positionLookup,
            ECB = ecbBOS,
            targetsArray = targetsArray
        };
        var handle = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        handle.Complete();

        //state.Dependency = new ProjectileHitJob()
        //{
        //    Projectiles = projectileLookup,
        //    Positions = positionLookup,
        //  /*  EnemiesHealth = healthLookup,
        //    Positions = positionLookup,
        //    HitLists = hitListLookup,*/
        //    ECB = ecbBOS
        //}.Schedule(simulation);

        //positionLookup.Update(ref state);
        //healthLookup.Update(ref state);
        //impactLookup.Update(ref state);
        //hitListLookup.Update(ref state);

        /* state.Dependency = new ProjectileHitJob
         {
             Projectiles = projectileLookup,
             Asteroids = asteroidLookup,
             Positions = positionLookup,            
             ECB = ecbBOS,
             targetsArray = targetsArray,
           //  HitLists = hitListLookup
         }.Schedule(simulation, state.Dependency);*/
        /*    ProjectileHitJob testJob = new ProjectileHitJob
            {
                Projectiles = projectileLookup,
                Asteroids = asteroidLookup,
                Positions = positionLookup,
                ECB = ecbBOS,
                targetsArray = targetsArray,
            }.Schedule(physicsWorld, PhysicsStep.Default);*/



        /*simulation.sc
        ProjectileHitJob testJobHandle = testJob.Schedule();*/
        // testJob.Complete();

        //state.CompleteDependency();
        //ecbBOS.Playback(EntityManager.EntityManagerDebug);
        // projectileLookup.Update(state.base);
        //WorldTransformLookup.Update(ref state);
    }

    // [BurstCompile]
    public struct ProjectileHitJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<LocalTransform> Positions;
        public ComponentLookup<ProjectileTag> Projectiles;
        public ComponentLookup<AsteroidTag> Asteroids;
        public NativeList<AsteroidHitList> targetsArray;
        //public ComponentLookup<Health> EnemiesHealth;

        public EntityCommandBuffer ECB;
        // public BufferLookup<AsteroidHitList> HitLists;

        public void Execute(TriggerEvent triggerEvent)
        {
            //Debug.Log("ProjectileHitJob.Execute");

            Entity projectile = Entity.Null;
            Entity asteroid = Entity.Null;

            // Identiy which entity is which
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

            if (projectile == null)
            {
                Debug.Log("projectile null");
                return;
            }
            if (asteroid == null)
            {
                Debug.Log("asteroid null");
                return;
            }

            Debug.Log("projectile");

            Projectiles.TryGetComponent(asteroid, out ProjectileTag projectileTag);
            Positions.TryGetComponent(asteroid, out LocalTransform projectilePosition);
            Asteroids.TryGetComponent(asteroid, out AsteroidTag asteroidTag);


            int length = targetsArray.Length;

           for (int i = 0; i < length; i++)
            {
                var possibleAsteroid = targetsArray[i];
                if (asteroid == possibleAsteroid.asteroidHit)
                {
                    possibleAsteroid.processCount++;
                    targetsArray[i] = possibleAsteroid;
                    return;
                }
            }
            targetsArray.Add( new AsteroidHitList
            {
                asteroidHit = asteroid,
                hitCount = 1,
                playerProjectileOwnerId = projectileTag.playerId,
                resourceGeneratedTypeId = asteroidTag.resourceType1,
                position = projectilePosition,
                processCount = 0
            });

            /*  if (EnemiesHealth.HasComponent(triggerEvent.EntityA))
                  enemy = triggerEvent.EntityA;
              if (EnemiesHealth.HasComponent(triggerEvent.EntityB))
                  enemy = triggerEvent.EntityB;*/

            /* // if its a pair of entity we don't want to process, exit
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
                 ECB.DestroyEntity(projectile);*/

        }

    }
}


/*
public partial class TriggerSystem : SystemBase
{
   private EndSimulationEntityCommandBufferSystem endECBSystem;
   private StepPhysicsWorld stepPhysicsWorld;

   protected override void OnCreate()
   {
       endECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
       stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
   }

   protected override void OnUpdate()
   {
       var triggerJob = new TriggerJob
       {
           allPickups = GetComponentDataFromEntity<PickupTag>(true),
           allPlayers = GetComponentDataFromEntity<PlayerTag>(),
           ecb = endECBSystem.CreateCommandBuffer()
       };

       Dependency = triggerJob.Schedule(stepPhysicsWorld.Simulation, Dependency);
       endECBSystem.AddJobHandleForProducer(Dependency);
   }
}


[BurstCompile]
struct TriggerJob : ITriggerEventsJob
{
   [ReadOnly] public ComponentLookup<PickupTag> allPickups;
   public ComponentLookup<PlayerTag> allPlayers;
   public EntityCommandBuffer ecb;

   public void Execute(TriggerEvent triggerEvent)
   {
       Entity entityA = triggerEvent.EntityA;
       Entity entityB = triggerEvent.EntityB;

       if (allPickups.HasComponent(entityA) && allPickups.HasComponent(entityB)) return;

       if (allPickups.HasComponent(entityA) && allPlayers.HasComponent(entityB))
       {
           ecb.DestroyEntity(entityA);
       }
       else if (allPickups.HasComponent(entityB) && allPlayers.HasComponent(entityA))
       {
           ecb.DestroyEntity(entityB);
       }
   }
}*/
