using Fusion;
using UnityEngine;

public class PlayerResources : NetworkBehaviour
{
    // Le proprietà [Networked] sono la base per la sincronizzazione dello stato in Fusion.
    // Solo il server (State Authority) può modificarle, e le modifiche vengono
    // automaticamente propagate a tutti i client.

    [Networked] public int Gold { get; set; }
    [Networked] public int Food { get; set; }
    [Networked] public int Wood { get; set; }
    [Networked] public int Stone { get; set; }

    [Header("Citizen Management")]
    [Networked] public int CitizensOnGold { get; set; }
    [Networked] public int CitizensOnFood { get; set; }
    [Networked] public int CitizensOnWood { get; set; }
    [Networked] public int CitizensOnStone { get; set; }
    [Networked] public int CitizensIdle { get; set; }

    // OnChange... callbacks possono essere aggiunti qui per aggiornare la UI quando un valore cambia, es:
    // [Networked(OnChanged = nameof(OnGoldChanged))] public int Gold { get; set; }
    // public static void OnGoldChanged(Changed<PlayerResources> changed) { ... }

    public override void Spawned()
    {
        // Questo metodo viene chiamato su server e client quando l'oggetto è stato spawnato in rete.
        // È il posto giusto per inizializzare i valori di rete.
        // Controlliamo se siamo il server prima di inizializzare per essere sicuri.
        if (Object.HasStateAuthority)
        {
            // Inizializza le risorse
            Gold = 1000;
            Food = 1000;
            Wood = 1000;
            Stone = 1000;

            // Inizializza i cittadini
            CitizensOnGold = 0;
            CitizensOnFood = 0;
            CitizensOnWood = 0;
            CitizensOnStone = 0;
            CitizensIdle = 5; // Inizia con 5 cittadini inattivi

            Debug.Log("Risorse e cittadini iniziali impostati per il giocatore.");
        }
    }

    #region Public Methods
    public bool CanAffordUnit(UnitType type)
    {
        switch (type)
        {
            case UnitType.Citizen: return Food >= 10;
            case UnitType.Infantry: return Gold >= 20 && Food >= 5;
            case UnitType.Spear: return Gold >= 20 && Food >= 5;
            case UnitType.Archer: return Gold >= 25 && Food >= 5;
            case UnitType.Crossbowman: return Gold >= 30 && Food >= 5;
            case UnitType.Horseman: return Gold >= 40 && Food >= 10;
            case UnitType.Knight: return Gold >= 60 && Food >= 15;
        }
        return false;
    }

    public bool CanAffordBuilding(int goldCost, int foodCost, int woodCost, int stoneCost)
    {
        return Gold >= goldCost && Food >= foodCost && Wood >= woodCost && Stone >= stoneCost;
    }

    public void RequestSpendForUnit(UnitType type) => RPC_SpendResourcesForUnit(type);
    public void RequestSpendForBuilding(int g, int f, int w, int s) => RPC_SpendResourcesForBuilding(g, f, w, s);
    public void RequestAssignCitizenToGold() => RPC_AssignCitizenToGold();
    public void RequestRemoveCitizenFromGold() => RPC_RemoveCitizenFromGold();
    public void RequestAssignCitizenToFood() => RPC_AssignCitizenToFood();
    public void RequestRemoveCitizenFromFood() => RPC_RemoveCitizenFromFood();
    public void RequestAssignCitizenToWood() => RPC_AssignCitizenToWood();
    public void RequestRemoveCitizenFromWood() => RPC_RemoveCitizenFromWood();
    public void RequestAssignCitizenToStone() => RPC_AssignCitizenToStone();
    public void RequestRemoveCitizenFromStone() => RPC_RemoveCitizenFromStone();

    #endregion

    #region RPCs

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SpendResourcesForUnit(UnitType type)
    {
        if (!CanAffordUnit(type)) return;

        switch (type)
        {
            case UnitType.Citizen:
                Food -= 10;
                break;
            case UnitType.Infantry:
                Gold -= 20;
                Food -= 5;
                break;
            case UnitType.Spear:
                Gold -= 20;
                Food -= 5;
                break;
            case UnitType.Archer:
                Gold -= 25;
                Food -= 5;
                break;
            case UnitType.Crossbowman:
                Gold -= 30;
                Food -= 5;
                break;
            case UnitType.Horseman:
                Gold -= 40;
                Food -= 10;
                break;
            case UnitType.Knight:
                Gold -= 60;
                Food -= 15;
                break;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SpendResourcesForBuilding(int goldCost, int foodCost, int woodCost, int stoneCost)
    {
        if (!CanAffordBuilding(goldCost, foodCost, woodCost, stoneCost)) return;

        Gold -= goldCost;
        Food -= foodCost;
        Wood -= woodCost;
        Stone -= stoneCost;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_AssignCitizenToGold()
    {
        if (CitizensIdle > 0)
        {
            CitizensIdle--;
            CitizensOnGold++;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RemoveCitizenFromGold()
    {
        if (CitizensOnGold > 0)
        {
            CitizensOnGold--;
            CitizensIdle++;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_AssignCitizenToFood()
    {
        if (CitizensIdle > 0)
        {
            CitizensIdle--;
            CitizensOnFood++;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RemoveCitizenFromFood()
    {
        if (CitizensOnFood > 0)
        {
            CitizensOnFood--;
            CitizensIdle++;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_AssignCitizenToWood()
    {
        if (CitizensIdle > 0)
        {
            CitizensIdle--;
            CitizensOnWood++;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RemoveCitizenFromWood()
    {
        if (CitizensOnWood > 0)
        {
            CitizensOnWood--;
            CitizensIdle++;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_AssignCitizenToStone()
    {
        if (CitizensIdle > 0)
        {
            CitizensIdle--;
            CitizensOnStone++;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RemoveCitizenFromStone()
    {
        if (CitizensOnStone > 0)
        {
            CitizensOnStone--;
            CitizensIdle++;
        }
    }

    #endregion

    [Header("Gather Settings")]
    [SerializeField] private float goldGatherRatePerCitizen = 10f;
    [SerializeField] private float foodGatherRatePerCitizen = 10f;
    [SerializeField] private float woodGatherRatePerCitizen = 10f;
    [SerializeField] private float stoneGatherRatePerCitizen = 10f;
    [SerializeField] private float gatherInterval = 2f;

    // Timer per la generazione delle risorse. Non necessitano di essere [Networked]
    // perché la logica di generazione viene eseguita interamente sul server.
    private float goldTimer = 0f;
    private float foodTimer = 0f;
    private float woodTimer = 0f;
    private float stoneTimer = 0f;

    // La logica di generazione delle risorse (che era in Update) verrebbe spostata qui,
    // dentro FixedUpdateNetwork, per essere eseguita solo sul server.
    public override void FixedUpdateNetwork()
    {
        // Questa logica deve essere eseguita solo dal server (State Authority)
        if (HasStateAuthority)
        {
            float deltaTime = Runner.DeltaTime;

            // Timer per cittadini assegnati all'oro
            if (CitizensOnGold > 0)
            {
                goldTimer += deltaTime;
                if (goldTimer >= gatherInterval)
                {
                    int ticks = Mathf.FloorToInt(goldTimer / gatherInterval);
                    int goldToAdd = Mathf.RoundToInt(ticks * CitizensOnGold * goldGatherRatePerCitizen);
                    if (goldToAdd > 0) Gold += goldToAdd;
                    goldTimer -= ticks * gatherInterval;
                }
            }

            // Timer per cittadini assegnati al cibo
            if (CitizensOnFood > 0)
            {
                foodTimer += deltaTime;
                if (foodTimer >= gatherInterval)
                {
                    int ticks = Mathf.FloorToInt(foodTimer / gatherInterval);
                    int foodToAdd = Mathf.RoundToInt(ticks * CitizensOnFood * foodGatherRatePerCitizen);
                    if (foodToAdd > 0) Food += foodToAdd;
                    foodTimer -= ticks * gatherInterval;
                }
            }

            // Timer per cittadini assegnati al legno
            if (CitizensOnWood > 0)
            {
                woodTimer += deltaTime;
                if (woodTimer >= gatherInterval)
                {
                    int ticks = Mathf.FloorToInt(woodTimer / gatherInterval);
                    int woodToAdd = Mathf.RoundToInt(ticks * CitizensOnWood * woodGatherRatePerCitizen);
                    if (woodToAdd > 0) Wood += woodToAdd;
                    woodTimer -= ticks * gatherInterval;
                }
            }

            // Timer per cittadini assegnati alla pietra
            if (CitizensOnStone > 0)
            {
                stoneTimer += deltaTime;
                if (stoneTimer >= gatherInterval)
                {
                    int ticks = Mathf.FloorToInt(stoneTimer / gatherInterval);
                    int stoneToAdd = Mathf.RoundToInt(ticks * CitizensOnStone * stoneGatherRatePerCitizen);
                    if (stoneToAdd > 0) Stone += stoneToAdd;
                    stoneTimer -= ticks * gatherInterval;
                }
            }
        }
    }
}
