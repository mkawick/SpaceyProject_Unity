using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct MoveControllerData : IComponentData
{
    public float3 direction;
    public float speed;
    public int turning;
    public float turnSpeed;
}
