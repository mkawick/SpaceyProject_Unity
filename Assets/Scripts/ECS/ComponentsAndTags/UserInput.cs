using Unity.Entities;
using UnityEngine;

//[GenerateAuthoringComponent] // watch video on this workflow.
public struct UserInput : IComponentData
{
    public KeyCode left, right, up, down;
    public KeyCode firing;
    public KeyCode rotateLeft, rotateRight;
}

