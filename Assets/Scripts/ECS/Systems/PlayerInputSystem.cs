using Unity.Entities;
using System;
using UnityEngine;

public partial class PlayerInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach( (ref MoveControllerData movement, ref AttackControllerData acd, in UserInput userInput) =>
        {
            if (Input.GetKey(userInput.left)) { movement.direction.x = -1; }
            else if (Input.GetKey(userInput.right)) { movement.direction.x = 1; }
            else movement.direction.x = 0;

            if (Input.GetKey(userInput.up)) { movement.direction.z = -1; }
            else if (Input.GetKey(userInput.down)) { movement.direction.z = 1; }
            else movement.direction.z = 0;

            if (Input.GetKey(userInput.firing)) { acd.isFiring = true; }
            else acd.isFiring = false;

            movement.turning = 0;
            if (Input.GetKey(userInput.rotateLeft)) 
            { 
                movement.turning--; 
            }
            if (Input.GetKey(userInput.rotateRight)) 
            { 
                movement.turning++; 
            }
        }).Run();
       
    }
}
