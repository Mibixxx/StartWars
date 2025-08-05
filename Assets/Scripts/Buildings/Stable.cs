using UnityEngine;

public class Stable : BuildingBase
{
    protected override void Start()
    {
        base.Start();
    }

    public override bool CanProduce(UnitType type)
    {
        return !IsUnderConstruction &&
               (type == UnitType.Horseman || type == UnitType.Knight) &&
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
        Debug.Log("Stable completed!");
    }
}
