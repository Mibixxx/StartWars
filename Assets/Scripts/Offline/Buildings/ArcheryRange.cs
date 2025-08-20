using UnityEngine;

public class ArcheryRange : BuildingBase
{
    protected override void Start()
    {
        base.Start();
    }

    public override bool CanProduce(UnitType type)
    {
        return !IsUnderConstruction &&
               (type == UnitType.Archer || type == UnitType.Crossbowman) &&
               HasRoomInQueue() &&
               UnitManager.Instance.CanAddUnit();
    }

    public override void ProduceUnit(UnitType type)
    {
        if (!CanProduce(type)) return;
        if (!ResourceManager.Instance.SpendResources(type)) return;

        GetQueue().Enqueue(type);
        UnitManager.Instance.EnqueueProduction(this, type);
    }

    protected override void OnConstructionComplete()
    {
        base.OnConstructionComplete();
        Debug.Log("Archery Range completed!");
    }
}
