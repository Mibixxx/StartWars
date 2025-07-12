using UnityEngine;

public class MilitaryBuilding : BuildingBase
{
    public Transform spawnPoint;

    public override bool CanProduce(UnitType type)
    {
        return type == UnitType.Soldier && !IsBusy && UnitManager.Instance.CanQueueUnit();
    }

    public override void ProduceUnit(UnitType type)
    {
        if (!CanProduce(type)) return;
        if (!ResourceManager.Instance.SpendResources(type)) return;

        UnitManager.Instance.EnqueueProduction(this, type);
    }

    public void SpawnSoldier()
    {
        GameObject soldier = Instantiate(UnitManager.Instance.soldierPrefab, spawnPoint.position, Quaternion.identity);
        UnitManager.Instance.AddSoldier(soldier.GetComponent<SoldierUnit>());
    }
}