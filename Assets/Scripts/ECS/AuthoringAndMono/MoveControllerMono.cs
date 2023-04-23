using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public class MoveControllerMono : MonoBehaviour 
{
    public float3 direction;
    public float speed;
    public float turnSpeed;
}

public class MoveControllerBaker : Baker<MoveControllerMono>
{
    public override void Bake(MoveControllerMono authoring)
    {
        var moveControllerEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(moveControllerEntity, new MoveControllerData
        {
            direction = authoring.direction,
            speed = authoring.speed,
            turnSpeed = authoring.turnSpeed
        });

    }
}