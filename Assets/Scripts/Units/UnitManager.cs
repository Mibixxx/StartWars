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

    private void Awake()
    {
        Instance = this;

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
        else if (type == UnitType.Soldier || type == UnitType.Pike)
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

        GameObject unitGO = Instantiate(prefab, ((MilitaryBuilding)building).spawnPoint.position, Quaternion.identity);

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

        Vector3 rallyPosition = GetNextFormationPosition();
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
            UnitType.Soldier => 5f,
            UnitType.Pike => 4f,
            _ => 0f
        };
    }

    public Vector3 GetNextFormationPosition()
    {
        int index = soldiers.Count;
        int soldiersPerRow = 3;
        float spacing = 0.3f;

        int row = index / soldiersPerRow;
        int col = index % soldiersPerRow;

        // Offset centrato rispetto al punto centrale
        float offsetX = (col - (soldiersPerRow - 1) / 2f) * spacing;
        float offsetZ = -row * spacing;

        // Calcola orientamento rispetto al rally point
        Vector3 right = soldierRallyPoint.right;
        Vector3 forward = soldierRallyPoint.forward;

        Vector3 offset = right * offsetX + forward * offsetZ;

        return soldierRallyPoint.position + offset;
    }

    public void RecalculateFormation()
    {
        int soldiersPerRow = 3;
        float spacing = 0.3f;

        Vector3 right = soldierRallyPoint.right;
        Vector3 forward = soldierRallyPoint.forward;

        for (int i = 0; i < soldiers.Count; i++)
        {
            int row = i / soldiersPerRow;
            int col = i % soldiersPerRow;

            float offsetX = (col - (soldiersPerRow - 1) / 2f) * spacing;
            float offsetZ = -row * spacing;

            Vector3 offset = right * offsetX + forward * offsetZ;
            Vector3 targetPosition = soldierRallyPoint.position + offset;

            soldiers[i].MoveToPosition(targetPosition);
        }
    }

    public void RemoveSoldier(MilitaryUnit soldier)
    {
        if (soldiers.Contains(soldier))
        {
            soldiers.Remove(soldier);
            OnSoldierCountChanged?.Invoke(soldiers.Count);
            RecalculateFormation();
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