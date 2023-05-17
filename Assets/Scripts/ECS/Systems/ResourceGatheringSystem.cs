using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public partial class ResourceGatheringSystem : SystemBase
{
    public Action<ResourceType, float> OnCollect;

    protected override void OnUpdate()
    {
        //throw new System.NotImplementedException();
        OnCollect?.Invoke(ResourceType.Copper, 0.1f);
    }


}
