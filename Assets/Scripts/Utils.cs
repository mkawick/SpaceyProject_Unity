using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

static class Utils
{
    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

    public static Vector3 GetRandomLocation(float originX, float originY, float minDist, float maxDist)
    {
        float randDist = UnityEngine.Random.Range(minDist, maxDist);
        float angle = UnityEngine.Random.Range(0, Mathf.PI * 2);

        float posX = randDist * Mathf.Cos(angle) + originX;
        float posZ = randDist * Mathf.Sin(angle) + originY;

        return new Vector3(posX, 0, posZ);
    }

    public static Vector3 GetRandomDirection(float minDist, float maxDist)
    {
        float range = Mathf.Abs(maxDist - minDist);
        Vector2 rand = UnityEngine.Random.insideUnitCircle;
        Vector2 scaled = rand * range;
        
        rand *= minDist;
        rand += scaled;
        return rand;
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

