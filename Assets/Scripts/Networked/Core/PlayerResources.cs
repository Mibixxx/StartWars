using Fusion;
using UnityEngine;
using System;
using System.Linq;

public class PlayerResources : NetworkBehaviour
{
    [Networked] public int Gold { get; set; }
    [Networked] public int Food { get; set; }
    [Networked] public int Wood { get; set; }
    [Networked] public int Stone { get; set; }

    [Networked] public int CitizensOnGold { get; set; }
    [Networked] public int CitizensOnFood { get; set; }
    [Networked] public int CitizensOnWood { get; set; }
    [Networked] public int CitizensOnStone { get; set; }
    [Networked] public int CitizensIdle { get; set; }

    // Eventi locali (solo per il client proprietario)
    public event Action<int> OnGoldChanged;
    public event Action<int> OnFoodChanged;
    public event Action<int> OnWoodChanged;
    public event Action<int> OnStoneChanged;

    public event Action<int> OnIdleCitizensChanged;
    public event Action OnWorkerDistributionChanged;

    // Cache valori precedenti
    private int prevGold, prevFood, prevWood, prevStone;
    private int prevIdle, prevGoldWorkers, prevFoodWorkers, prevWoodWorkers, prevStoneWorkers;

    public float resourceTickRate = 1f; // ogni 1 secondo
    private float resourceTickTimer = 0f;
    public int resourcePerCitizen = 10; // quanto produce un cittadino ogni tick

    public static PlayerResources LocalPlayerResources { get; private set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
            LocalPlayerResources = this;

        if (Object.HasStateAuthority)
        {
            Gold = 1000;
            Food = 1000;
            Wood = 1000;
            Stone = 1000;

            CitizensIdle = 0;
            CitizensOnGold = 0;
            CitizensOnFood = 0;
            CitizensOnWood = 0;
            CitizensOnStone = 0;
        }

        // Inizializza cache
        prevGold = Gold;
        prevFood = Food;
        prevWood = Wood;
        prevStone = Stone;

        prevIdle = CitizensIdle;
        prevGoldWorkers = CitizensOnGold;
        prevFoodWorkers = CitizensOnFood;
        prevWoodWorkers = CitizensOnWood;
        prevStoneWorkers = CitizensOnStone;
    }

    public override void FixedUpdateNetwork()
    {        
        if (Object.HasStateAuthority)
        {
            resourceTickTimer += Runner.DeltaTime;
            if (resourceTickTimer >= resourceTickRate)
            {
                resourceTickTimer = 0f;

                Gold += CitizensOnGold * resourcePerCitizen;
                Food += CitizensOnFood * resourcePerCitizen;
                Wood += CitizensOnWood * resourcePerCitizen;
                Stone += CitizensOnStone * resourcePerCitizen;
            }
        }

        // Aggiorna solo se è il client proprietario
        if (!Object.HasInputAuthority)
            return;

        // Risorse
        if (Gold != prevGold) { OnGoldChanged?.Invoke(Gold); prevGold = Gold; }
        if (Food != prevFood) { OnFoodChanged?.Invoke(Food); prevFood = Food; }
        if (Wood != prevWood) { OnWoodChanged?.Invoke(Wood); prevWood = Wood; }
        if (Stone != prevStone) { OnStoneChanged?.Invoke(Stone); prevStone = Stone; }

        // Cittadini inattivi
        if (CitizensIdle != prevIdle) { OnIdleCitizensChanged?.Invoke(CitizensIdle); prevIdle = CitizensIdle; }

        // Distribuzione lavoratori
        if (CitizensOnGold != prevGoldWorkers ||
            CitizensOnFood != prevFoodWorkers ||
            CitizensOnWood != prevWoodWorkers ||
            CitizensOnStone != prevStoneWorkers)
        {
            OnWorkerDistributionChanged?.Invoke();

            prevGoldWorkers = CitizensOnGold;
            prevFoodWorkers = CitizensOnFood;
            prevWoodWorkers = CitizensOnWood;
            prevStoneWorkers = CitizensOnStone;
        }
    }

    public bool SpendResources(UnitType type)
    {
        if (!Object.HasStateAuthority)
            return false; // Solo il server può scalare le risorse

        switch (type)
        {
            case UnitType.Citizen:
                if (Food >= 10)
                {
                    Food -= 10;
                    return true;
                }
                break;

            case UnitType.Infantry:
                if (Gold >= 20 && Food >= 5)
                {
                    Gold -= 20;
                    Food -= 5;
                    return true;
                }
                break;

            case UnitType.Spear:
                if (Gold >= 20 && Food >= 5)
                {
                    Gold -= 20;
                    Food -= 5;
                    return true;
                }
                break;

            case UnitType.Archer:
                if (Gold >= 25 && Food >= 5)
                {
                    Gold -= 25;
                    Food -= 5;
                    return true;
                }
                break;

            case UnitType.Crossbowman:
                if (Gold >= 30 && Food >= 5)
                {
                    Gold -= 30;
                    Food -= 5;
                    return true;
                }
                break;

            case UnitType.Horseman:
                if (Gold >= 40 && Food >= 10)
                {
                    Gold -= 40;
                    Food -= 10;
                    return true;
                }
                break;

            case UnitType.Knight:
                if (Gold >= 60 && Food >= 15)
                {
                    Gold -= 60;
                    Food -= 15;
                    return true;
                }
                break;
        }

        return false;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestBuild(NetworkObject tileObject, string buildingName)
    {
        if (tileObject == null)
        {
            Debug.LogWarning("RPC_RequestBuild: TileObject nullo");
            return;
        }

        Tile_Networked tile = tileObject.GetComponent<Tile_Networked>();
        if (tile == null)
        {
            Debug.LogWarning("RPC_RequestBuild: Tile non valida");
            return;
        }

        if (tile.HasBuilding)
        {
            Debug.Log("RPC_RequestBuild: Tile già occupata");
            return;
        }

        BuildingData data = InputManager_Networked.Instance.GetBuildingDataByName(buildingName);
        if (data == null)
        {
            Debug.LogWarning($"RPC_RequestBuild: BuildingData non trovato per {buildingName}");
            return;
        }

        PlayerResources ownerResources = Object.GetComponent<PlayerResources>();
        if (ownerResources == null)
        {
            Debug.LogWarning("RPC_RequestBuild: PlayerResources non trovato sul player");
            return;
        }

        // Controllo risorse
        if (ownerResources.Gold < data.goldCost ||
            ownerResources.Food < data.foodCost ||
            ownerResources.Wood < data.woodCost ||
            ownerResources.Stone < data.stoneCost)
        {
            Debug.Log($"RPC_RequestBuild: Risorse insufficienti per costruire {data.buildingName}");
            return;
        }

        // Scala le risorse
        ownerResources.Gold -= data.goldCost;
        ownerResources.Food -= data.foodCost;
        ownerResources.Wood -= data.woodCost;
        ownerResources.Stone -= data.stoneCost;

        // Spawn dell'edificio lato server
        NetworkObject buildingNO = Runner.Spawn(
            data.prefab,
            tile.transform.position, // posizione corretta della tile
            Quaternion.identity,
            Object.InputAuthority
        );

        if (buildingNO == null)
        {
            Debug.LogError($"RPC_RequestBuild: Fallito spawn edificio {data.buildingName}");
            return;
        }

        BuildingBase_Networked buildingBase = buildingNO.GetComponent<BuildingBase_Networked>();
        if (buildingBase != null)
        {
            buildingBase.buildingData = data;
            tile.SetBuilding(buildingNO);
        }

        // Setta l’owner lato server
        var buildingComponent = buildingNO.GetComponent<BuildingBase_Networked>();
        if (buildingComponent != null)
            buildingComponent.SetOwner(this);

        Debug.Log($"[RPC_RequestBuild] Edificio {data.buildingName} creato alla posizione {tile.transform.position}");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestProduceCitizen()
    {
        // Prendo tutte le TownHall del player sul server
        var townHalls = UnityEngine.Object.FindObjectsByType<TownHall_Networked>(FindObjectsSortMode.None)
            .Where(b => b.OwnerResources == this && b.HasRoomInQueue())
            .OrderBy(b => b.GetQueue().Count)
            .ToList();

        if (townHalls.Count == 0) return;

        townHalls.First().ProduceUnit(UnitType.Citizen);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestProduceMilitaryUnit(UnitType type)
    {
        // Prende tutti gli edifici del player
        var buildings = UnityEngine.Object.FindObjectsByType<BuildingBase_Networked>(UnityEngine.FindObjectsSortMode.None)
            .Where(b => b.OwnerResources == this && b.CanProduce(type))
            .OrderBy(b => b.GetQueue().Count)
            .ToList();

        if (buildings.Count == 0) return;

        var targetBuilding = buildings.First();

        // Controllo risorse
        if (!SpendResources(type)) return;

        targetBuilding.ProduceUnit(type);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestCitizenAssignment(ResourceType type, bool add)
    {
        switch (type)
        {
            case ResourceType.Gold:
                if (add && CitizensIdle > 0)
                {
                    CitizensOnGold++;
                    CitizensIdle--;
                }
                else if (!add && CitizensOnGold > 0)
                {
                    CitizensOnGold--;
                    CitizensIdle++;
                }
                break;

            case ResourceType.Food:
                if (add && CitizensIdle > 0)
                {
                    CitizensOnFood++;
                    CitizensIdle--;
                }
                else if (!add && CitizensOnFood > 0)
                {
                    CitizensOnFood--;
                    CitizensIdle++;
                }
                break;

            case ResourceType.Wood:
                if (add && CitizensIdle > 0)
                {
                    CitizensOnWood++;
                    CitizensIdle--;
                }
                else if (!add && CitizensOnWood > 0)
                {
                    CitizensOnWood--;
                    CitizensIdle++;
                }
                break;

            case ResourceType.Stone:
                if (add && CitizensIdle > 0)
                {
                    CitizensOnStone++;
                    CitizensIdle--;
                }
                else if (!add && CitizensOnStone > 0)
                {
                    CitizensOnStone--;
                    CitizensIdle++;
                }
                break;
        }
    }
}
