using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class ArcheryRange_Networked : BuildingBase_Networked
{
    // Lista delle unità producibili da questa struttura
    [SerializeField] private List<UnitType> producibleUnits = new() { UnitType.Archer, UnitType.Crossbowman };

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

    // La produzione viene richiesta dal client locale e processata lato server
    public void RequestProduceUnit(UnitType type)
    {
        if (!CanProduce(type))
        {
            Debug.LogWarning($"Impossibile produrre {type} ora.");
            return;
        }

        // Solo il client locale con InputAuthority invia la richiesta al server
        if (ownerResources != null && ownerResources.Object.HasInputAuthority)
        {
            ownerResources.RPC_RequestProduceMilitaryUnit(type);
        }
    }

    protected override void OnConstructionComplete()
    {
        base.OnConstructionComplete();
        Debug.Log("Archery Range completato!");
    }
}
