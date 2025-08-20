using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class Stable_Networked : BuildingBase_Networked
{
    // Lista delle unità producibili da questa struttura
    [SerializeField] private List<UnitType> producibleUnits = new() { UnitType.Horseman, UnitType.Knight };

    protected override void Start()
    {
        base.Start();
    }

    public override bool CanProduce(UnitType type)
    {
        return !IsUnderConstruction &&
               producibleUnits.Contains(type) &&
               HasRoomInQueue() &&
               UnitManager_Networked.Instance != null &&
               UnitManager_Networked.Instance.CanAddUnit();
    }

    // La produzione viene richiesta dal client locale e gestita lato server
    public void RequestProduceUnit(UnitType type)
    {
        if (!CanProduce(type))
        {
            Debug.LogWarning($"Impossibile produrre {type} ora.");
            return;
        }

        // Solo il client con InputAuthority invia la richiesta RPC al server
        if (ownerResources != null && ownerResources.Object.HasInputAuthority)
        {
            ownerResources.RPC_RequestProduceMilitaryUnit(type);
        }
    }

    protected override void OnConstructionComplete()
    {
        base.OnConstructionComplete();
        Debug.Log("Stable completato!");
    }
}
