using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class UnitManager_Networked : NetworkBehaviour
{
    public static UnitManager_Networked Instance;

    [Header("Unit Limits")]
    public int MaxUnits = 50;  // Massimo totale unità
    public int MaxQueue = 15;  // Massimo coda globale

    private int currentCitizens = 0;
    private List<MilitaryUnit_Networked> soldiersP1 = new();
    private List<MilitaryUnit_Networked> soldiersP2 = new();

    // Coda di produzione (edificio + tipo unità)
    private Queue<(BuildingBase_Networked building, UnitType type)> productionQueue = new();

    [Header("Unit Prefabs")]
    public List<UnitPrefabEntry> unitPrefabsList = new();
    private Dictionary<UnitType, GameObject> unitPrefabs;

    public Transform soldierRallyPoint_Player1;
    public Transform soldierRallyPoint_Player2;

    private FormationManager formationManagerP1;
    private FormationManager formationManagerP2;

    private void Awake()
    {
        Instance = this;

        formationManagerP1 = new FormationManager(soldiersP1);
        formationManagerP2 = new FormationManager(soldiersP2);

        // Inizializza dizionario prefab
        unitPrefabs = new Dictionary<UnitType, GameObject>();
        foreach (var entry in unitPrefabsList)
        {
            if (!unitPrefabs.ContainsKey(entry.unitType))
                unitPrefabs.Add(entry.unitType, entry.prefab);
            else
                Debug.LogWarning($"UnitType duplicato: {entry.unitType}");
        }
    }

    private void Update()
    {
        if (Runner == null || !Runner.IsServer) return;
        if (productionQueue == null || productionQueue.Count == 0) return;

        int iterations = productionQueue.Count;
        for (int i = 0; i < iterations; i++)
        {
            var (building, type) = productionQueue.Dequeue();

            if (building == null)
            {
                continue;
            }

            if (!building.IsBusy)
            {
                building.StartCoroutine(ProductionCoroutine(building, type));
            }
            else
            {
                productionQueue.Enqueue((building, type));
            }
        }
    }

    private IEnumerator ProductionCoroutine(BuildingBase_Networked building, UnitType type)
    {
        building.SetBusy(true);

        // Tempo di produzione
        yield return new WaitForSeconds(GetProductionTime(type));

        // Lato server: produzione
        if (type == UnitType.Citizen)
        {
            AddCitizen(building.OwnerResources);
        }
        else
        {
            SpawnUnit(building, type, building.OwnerResources);
        }

        // Rimuove dalla coda dell'edificio in sicurezza
        var queue = building.GetQueue();
        if (queue != null && queue.Count > 0)
            queue.Dequeue();

        building.SetBusy(false);
    }

    #region PRODUCTION HELPERS

    public bool CanAddUnit() => TotalUnits < MaxUnits;
    public bool CanQueueUnit() => productionQueue.Count < MaxQueue && CanAddUnit();

    public void EnqueueProduction(BuildingBase_Networked building, UnitType type)
    {
        if (building == null)
        {
            Debug.LogError("Tentativo di enqueuare una produzione su edificio null!");
            return;
        }
        productionQueue.Enqueue((building, type));
    }

    public void SpawnUnit(BuildingBase_Networked building, UnitType type, PlayerResources owner)
    {
        if (building == null)
        {
            Debug.LogError("SpawnUnit fallito: building null");
            return;
        }

        if (!unitPrefabs.TryGetValue(type, out GameObject prefab))
        {
            Debug.LogError($"Prefab mancante per {type}");
            return;
        }

        Vector3 spawnPos = building.spawnPoint != null ? building.spawnPoint.position : building.transform.position;

        // Spawn Networked
        NetworkObject netObj = Runner.Spawn(prefab, spawnPos, Quaternion.identity, owner.Object.InputAuthority);
        if (netObj == null)
        {
            Debug.LogError($"Runner.Spawn ha restituito null per {type}");
            return;
        }

        MilitaryUnit_Networked unit = netObj.GetComponent<MilitaryUnit_Networked>();
        if (unit == null)
        {
            Debug.LogError($"Prefab {type} non ha MilitaryUnit!");
            return;
        }

        bool ownerIsHost = owner.Object.InputAuthority == Runner.LocalPlayer;

        if (ownerIsHost)
        {
            AddSoldier(unit, soldiersP1, formationManagerP1, soldierRallyPoint_Player1.forward, soldierRallyPoint_Player1.position);
        }
        else
        {
            AddSoldier(unit, soldiersP2, formationManagerP2, soldierRallyPoint_Player2.forward, soldierRallyPoint_Player2.position);
        }
    }

    #endregion

    #region UNIT COUNT

    public void AddCitizen(PlayerResources owner)
    {
        if (owner == null)
        {
            Debug.LogWarning("PlayerResources null in AddCitizen!");
            return;
        }

        // Incrementa i cittadini inattivi (solo lato server)
        if (owner.HasStateAuthority)
        {
            owner.CitizensIdle++;
        }

        // Aggiorna conteggio totale unità (locale al manager)
        currentCitizens++;
    }

    private void AddSoldier(MilitaryUnit_Networked soldier, List<MilitaryUnit_Networked> list, FormationManager manager, Vector3 forward, Vector3 centerPoint)
    {
        if (Runner == null || !Runner.IsServer) return;
        if (soldier == null) return;

        list.Add(soldier);

        if (manager != null && soldier.Object.HasStateAuthority)
        {
            Vector3 targetPos = manager.GetNextFormationPosition(centerPoint, forward);
            soldier.MoveToPosition(targetPos);
        }
    }

    public void RemoveSoldier(MilitaryUnit_Networked soldier, PlayerRef ownerRef)
    {
        if (soldier == null) return;

        // Determina quale lista e manager usare
        List<MilitaryUnit_Networked> list = null;
        FormationManager manager = null;
        Vector3 centerPoint = Vector3.zero;
        Vector3 forward = Vector3.forward;

        if (ownerRef.PlayerId == 0 && soldiersP1.Contains(soldier))
        {
            list = soldiersP1;
            manager = formationManagerP1;
            if (soldierRallyPoint_Player1 != null)
            {
                centerPoint = soldierRallyPoint_Player1.position;
                forward = soldierRallyPoint_Player1.forward;
            }
        }
        else if (ownerRef.PlayerId == 1 && soldiersP2.Contains(soldier))
        {
            list = soldiersP2;
            manager = formationManagerP2;
            if (soldierRallyPoint_Player2 != null)
            {
                centerPoint = soldierRallyPoint_Player2.position;
                forward = soldierRallyPoint_Player2.forward;
            }
        }

        if (list != null)
        {
            list.Remove(soldier);

            if (manager != null)
                manager.MoveUnitsToFormation(centerPoint, forward);
        }
    }

    #endregion

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

    public int GetCitizenCount() => currentCitizens;
    public int GetSoldierCountP1() => soldiersP1.Count;
    public int GetSoldierCountP2() => soldiersP2.Count;
    public int TotalUnits => currentCitizens + soldiersP1.Count + soldiersP2.Count;
    public int QueueCount => productionQueue.Count;
}
