using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct AttackControllerData : IComponentData
{
    public bool isFiring;
    public int playerId;
    // public NativeArray<bool> isFiring;
    ///Unity.Entities.FixedB
}
