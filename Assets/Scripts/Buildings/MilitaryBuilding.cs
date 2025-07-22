using UnityEngine;

public class MilitaryBuilding : BuildingBase
{
    public Transform spawnPoint;

    protected override void Start()
    {
        constructionTime = 5f;
        base.Start();
    }

    public override bool CanProduce(UnitType type)
    {
        return !IsUnderConstruction &&
               (type == UnitType.Soldier || type == UnitType.Pike) &&
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
        Debug.Log("Military building completed!");
    }
}
