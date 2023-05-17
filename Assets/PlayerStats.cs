using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PlayerStats : MonoBehaviour
{
    static int numResources = Enum.GetNames(typeof(ResourceType)).Length;
    float [] resources = new float[numResources];
    public TMPro.TMP_Text []fields = new TMPro.TMP_Text[numResources];
    // Start is called before the first frame update
    void Start()
    {
        //AddResource(ResourceType.Iron, 10);
        var resourceGathering = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ResourceGatheringSystem>();
        resourceGathering.OnCollect += AddResource;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddResource(ResourceType type, float quantity)
    {
        int index = (int)type;
        resources[index] += quantity;
        fields[index].text = (Math.Floor(resources[index] * 10) / 10).ToString();
    }
}
