using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;
    public event System.Action<int> OnCitizenCountChanged;
    public event System.Action<int> OnSoldierCountChanged;

    public int MaxUnits = 50;
    public int MaxQueue = 15;

    private int currentCitizens = 0;
    private List<MilitaryUnit> soldiers = new();

    private Queue<(BuildingBase building, UnitType type)> productionQueue = new();

    public List<UnitPrefabEntry> unitPrefabsList = new();
    private Dictionary<UnitType, GameObject> unitPrefabs;
    public Transform soldierRallyPoint;
    private SoldierFormationManager formationManager;

    private void Awake()
    {
        Instance = this;

        formationManager = new SoldierFormationManager(soldierRallyPoint, soldiers);

        unitPrefabs = new Dictionary<UnitType, GameObject>();
        foreach (var entry in unitPrefabsList)
        {
            if (!unitPrefabs.ContainsKey(entry.unitType))
            {
                unitPrefabs.Add(entry.unitType, entry.prefab);
            }
            else
            {
                Debug.LogWarning($"UnitType duplicato nella lista: {entry.unitType}");
            }
        }
    }

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

        yield return new WaitForSeconds(GetProductionTime(type));

        if (type == UnitType.Citizen)
            AddCitizen();
        else if (
        type == UnitType.Infantry ||
        type == UnitType.Spear ||
        type == UnitType.Archer ||
        type == UnitType.Crossbowman ||
        type == UnitType.Horseman ||
        type == UnitType.Knight
    )
            SpawnUnit(building, type);

        // Rimuove l'unità dalla coda locale
        building.GetQueue().Dequeue();

        building.SetBusy(false);
    }

    public bool CanAddUnit() => TotalUnits < MaxUnits;
    public bool CanQueueUnit() => productionQueue.Count < MaxQueue && CanAddUnit();

    public void EnqueueProduction(BuildingBase building, UnitType type)
    {
        productionQueue.Enqueue((building, type));
    }

    public void SpawnUnit(BuildingBase building, UnitType type)
    {
        if (!unitPrefabs.TryGetValue(type, out GameObject prefab))
        {
            Debug.LogError($"Prefab mancante per {type}");
            return;
        }

        GameObject unitGO = Instantiate(prefab, building.spawnPoint.position, Quaternion.identity);

        MilitaryUnit unit = unitGO.GetComponent<MilitaryUnit>();
        if (unit == null)
        {
            Debug.LogError($"Il prefab {type} non ha un componente MilitaryUnit!");
            return;
        }

        AddSoldier(unit); // anche se è un arciere o cavaliere

        if (soldierRallyPoint == null)
        {
            Debug.LogError("Rally point NULL!");
            return;
        }

        Vector3 rallyPosition = formationManager.GetNextFormationPosition();
        unit.MoveToPosition(rallyPosition);
    }

    public void AddCitizen()
    {
        currentCitizens++;
        ResourceManager.Instance.UpdateCitizenCount(currentCitizens);
        OnCitizenCountChanged?.Invoke(currentCitizens);
        ResourceManager.Instance.AddIdleCitizen();
    }

    public void AddSoldier(MilitaryUnit soldier)
    {
        soldiers.Add(soldier);
        OnSoldierCountChanged?.Invoke(soldiers.Count);
    }

    private float GetProductionTime(UnitType type)
    {
        return type switch
        {
            UnitType.Citizen => 3f,
            UnitType.Infantry => 5f,
            UnitType.Spear => 4f,
            UnitType.Archer => 3f,
            UnitType.Crossbowman => 2f,
            UnitType.Horseman => 3f,
            UnitType.Knight => 3f,
            _ => 0f
        };
    }

    public void RemoveSoldier(MilitaryUnit soldier)
    {
        if (soldiers.Contains(soldier))
        {
            soldiers.Remove(soldier);
            OnSoldierCountChanged?.Invoke(soldiers.Count);
            formationManager.RecalculateFormation();
        }
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