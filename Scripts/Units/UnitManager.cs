using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    public int MaxUnits = 50;
    public int MaxQueue = 15;

    private int currentCitizens = 0;
    private List<SoldierUnit> soldiers = new();

    private Queue<(BuildingBase building, UnitType type)> productionQueue = new();

    public GameObject soldierPrefab;

    private void Awake() => Instance = this;

    private void Update()
    {
        if (productionQueue.Count > 0)
        {
            var (building, type) = productionQueue.Peek();
            if (!building.IsBusy)
            {
                productionQueue.Dequeue();
                building.StartCoroutine(ProductionCoroutine(building, type));
            }
        }
    }

    private IEnumerator ProductionCoroutine(BuildingBase building, UnitType type)
    {
        building.SetBusy(true);
        yield return new WaitForSeconds(building.ProductionTime);

        if (type == UnitType.Citizen)
            AddCitizen();
        else if (type == UnitType.Soldier)
            (building as MilitaryBuilding)?.SpawnSoldier();

        building.SetBusy(false);
    }

    public bool CanAddUnit() => TotalUnits < MaxUnits;
    public bool CanQueueUnit() => productionQueue.Count < MaxQueue && CanAddUnit();

    public void EnqueueProduction(BuildingBase building, UnitType type)
    {
        productionQueue.Enqueue((building, type));
    }

    public void AddCitizen()
    {
        currentCitizens++;
        ResourceManager.Instance.UpdateCitizenCount(currentCitizens);
    }

    public void AddSoldier(SoldierUnit soldier)
    {
        soldiers.Add(soldier);
    }

    public void CommandAllSoldiersToAttack()
    {
        foreach (var soldier in soldiers)
        {
            soldier.SwitchToAttack();
        }
    }

    public int GetCitizenCount() => currentCitizens;
    public int GetSoldierCount() => soldiers.Count;
    public int TotalUnits => currentCitizens + soldiers.Count;
    public int QueueCount => productionQueue.Count;
}