using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class ShipMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        Entities.ForEach( (ref LocalTransform transform, in MoveControllerData moveData) => 
        {
            transform.Position.x += moveData.direction.x * deltaTime * moveData.speed;
            transform.Position.z += moveData.direction.z * deltaTime * moveData.speed;

            if(moveData.turning != 0)
            {
                var res = transform.RotateY( moveData.turnSpeed * moveData.turning  * deltaTime);
                transform = res;
            }
        } ).Run();
    }
}
