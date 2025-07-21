using UnityEngine;

public class MilitaryBuilding : BuildingBase
{
    public Transform spawnPoint;

    public override bool CanProduce(UnitType type)
    {
        return (type == UnitType.Soldier || type == UnitType.Pike) &&
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

    
}
