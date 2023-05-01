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
        // You don't specify a size because the buffer will grow as needed.
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);


        // The ECB is captured by the ForEach job.
        // Until completed, the job owns the ECB's job safety handle.
        Entities
            .ForEach((ref ProjectileFiringData projectileData, in AttackControllerData acd, in Entity e,  in LocalTransform transform) =>
            {
                if (projectileData.projectile == null)
                    return;
                if (acd.isFiring == true)
                {
                    if (projectileData.spawnRate < projectileData.currentTime)
                    {
                        projectileData.currentTime = 0;
                        var instance = ecb.Instantiate(projectileData.projectile);

                        //float scale = 0.2;//TODO, convert to a pransform pass thru at baking time

                        float3 shipRotation = -transform.Right();    // no sure why but the ship is pointing the wrong way... should be transform.Forward();

                        var newPos = transform.Position + shipRotation * 2;
                        ecb.SetComponent(instance, new LocalTransform { Position = newPos, Scale = projectileData.scale, Rotation = transform.Rotation });
                        ecb.SetComponent(instance, new ProjectileTag { playerId = acd.playerId });

                        ecb.AddComponent<MoveControllerData>(instance, new MoveControllerData { direction = shipRotation, speed = 5, turnSpeed = 0.0f });
                    }
                }
                projectileData.currentTime += deltaTime;
            }).Schedule();

        Dependency.Complete();

        // Now that the job is completed, you can enact the changes.
        // Note that Playback can only be called on the main thread.
        ecb.Playback(EntityManager);

        // You are responsible for disposing of any ECB you create.
        ecb.Dispose();
    }
    public static float3 unityQuaternionToEuler(quaternion q2)
    {
        float4 q1 = q2.value;

        float sqw = q1.w * q1.w;
        float sqx = q1.x * q1.x;
        float sqy = q1.y * q1.y;
        float sqz = q1.z * q1.z;
        float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
        float test = q1.x * q1.w - q1.y * q1.z;
        float3 v;

        if (test > 0.4995f * unit)
        { // singularity at north pole
            v.y = 2f * math.atan2(q1.y, q1.x);
            v.x = math.PI / 2;
            v.z = 0;
            return NormalizeAngles(math.degrees(v));
        }
        if (test < -0.4995f * unit)
        { // singularity at south pole
            v.y = -2f * math.atan2(q1.y, q1.x);
            v.x = -math.PI / 2;
            v.z = 0;
            return NormalizeAngles(math.degrees(v));
        }

        quaternion q3 = new quaternion(q1.w, q1.z, q1.x, q1.y);
        float4 q = q3.value;

        v.y = math.atan2(2f * q.x * q.w + 2f * q.y * q.z, 1 - 2f * (q.z * q.z + q.w * q.w));   // Yaw
        v.x = math.asin(2f * (q.x * q.z - q.w * q.y));                                         // Pitch
        v.z = math.atan2(2f * q.x * q.y + 2f * q.z * q.w, 1 - 2f * (q.y * q.y + q.z * q.z));   // Roll

        return NormalizeAngles(math.degrees(v));
    }

    static float3 NormalizeAngles(float3 angles)
    {
        angles.x = NormalizeAngle(angles.x);
        angles.y = NormalizeAngle(angles.y);
        angles.z = NormalizeAngle(angles.z);
        return angles;
    }

    static float NormalizeAngle(float angle)
    {
        while (angle > 360)
            angle -= 360;
        while (angle < 0)
            angle += 360;
        return angle;
    }
}
