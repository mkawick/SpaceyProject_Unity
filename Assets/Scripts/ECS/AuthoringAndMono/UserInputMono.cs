using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

public class UserInputMono : MonoBehaviour 
{
    public KeyCode left, right, up, down;
    public KeyCode fire;// needs array
    public KeyCode rotateLeft, rotateRight;
    //void Foo() { left = KeyCode.UpArrow; }
}

public class UserInputBaker : Baker<UserInputMono>
{
    public override void Bake(UserInputMono authoring)
    {
        var userInputEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(userInputEntity, new UserInput
        {
            left = authoring.left,
            right = authoring.right,
            up = authoring.up,
            down = authoring.down,
            firing = authoring.fire,
            rotateLeft = authoring.rotateLeft, 
            rotateRight = authoring.rotateRight
        });

    }
}