using Unity.Entities;

//[TemporaryBakingType]
public struct AsteroidTag : IComponentData
{
    public float percentDrop;
    public int resourceType1; // resources and percentages
    public float percentToDrop1;
}
