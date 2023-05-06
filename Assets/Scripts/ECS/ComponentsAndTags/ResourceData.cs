using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct ResourceData : IComponentData
{
    public ResourceType type;
    public int quantity;
    public int playerWhoSpawned; // -1 is no player
    // public NativeArray<bool> isFiring;
    ///Unity.Entities.FixedB
}
