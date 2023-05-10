using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct EntityElement : IBufferElementData
{
    public Entity resource;
}

public struct ResourceSpawnerData : IComponentData
{
    public Entity resource1;
}