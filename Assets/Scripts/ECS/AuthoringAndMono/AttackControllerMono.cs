using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public class AttackControllerMono : MonoBehaviour
{
    public int numWeapons;
}


public class AttackControllerBaker : Baker<AttackControllerMono>
{
    public override void Bake(AttackControllerMono authoring)
    {
        var attackControllerEntity = GetEntity(TransformUsageFlags.Dynamic);
        if (authoring.numWeapons == 1)
        {
            AddComponent(attackControllerEntity, new AttackControllerData
            {
                isFiring = false
            });
        }
    }
}