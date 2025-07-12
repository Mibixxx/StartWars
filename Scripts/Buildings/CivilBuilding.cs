using UnityEngine;

public class CivilBuilding : BuildingBase
{
    public override bool CanProduce(UnitType type)
    {
        return type == UnitType.Citizen && !IsBusy && UnitManager.Instance.CanQueueUnit();
    }

    public override void ProduceUnit(UnitType type)
    {
        if (!CanProduce(type)) return;
        if (!ResourceManager.Instance.SpendResources(type)) return;

        UnitManager.Instance.EnqueueProduction(this, type);
    }
}