using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct AgentData : IComponentData
{
    public int agentId;
    public int playerId;
    public int teamId;

    //public float speed;
    //public int turning;
    //public float turnSpeed;

    //public float acceleration;
    //public float accelerationSpeed;
}

