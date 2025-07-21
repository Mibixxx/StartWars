using UnityEngine;

public class CivilBuilding : BuildingBase
{
    public override bool CanProduce(UnitType type)
    {
        return type == UnitType.Citizen &&
               HasRoomInQueue() &&
               UnitManager.Instance.CanAddUnit(); // globale
    }

    public override void ProduceUnit(UnitType type)
    {
        if (!CanProduce(type)) return;
        if (!ResourceManager.Instance.SpendResources(type)) return;

        localQueue.Enqueue(type);
        UnitManager.Instance.EnqueueProduction(this, type);
    }
}
