using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// todo.. make this array
// https://coffeebraingames.wordpress.com/2020/12/20/the-different-lists-allowed-in-icomponentdata/
public struct MinerTag : IComponentData
{
    private float Aluminum, Copper, Iron, Izithrium;
   // private float[] amountMined;// = new float[10];// = new float [(int)ResourceType.NumTypes];

    public float this[ResourceType type] 
    {
        get 
        {
            if(type == ResourceType.Aluminum)
                return Aluminum;
            if(type == ResourceType.Copper)
                return Copper;
            if(type == ResourceType.Iron)
                return Iron;

            return Izithrium;
        }
        set 
        {
            if (type == ResourceType.Aluminum)
            { 
                Aluminum = value;
                return; 
            }
            if (type == ResourceType.Copper)
            {
                Copper = value;
                return;
            }
            if (type == ResourceType.Iron)
            {
                Iron = value;
                return;
            }
            Izithrium = value;
        }       
    }
}
