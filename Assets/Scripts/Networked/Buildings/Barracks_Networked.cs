using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class Barracks_Networked : BuildingBase_Networked
{
    // Lista delle unità che questa struttura può produrre
    [SerializeField] private List<UnitType> producibleUnits = new() { UnitType.Infantry, UnitType.Spear };

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

    // In realtà la produzione avviene lato server tramite RPC
    public void RequestProduceUnit(UnitType type)
    {
        if (!CanProduce(type))
        {
            Debug.LogWarning($"Impossibile produrre {type} ora.");
            return;
        }

        // Solo il client locale invia la richiesta
        if (ownerResources != null && ownerResources.Object.HasInputAuthority)
        {
            ownerResources.RPC_RequestProduceMilitaryUnit(type);
        }
    }

    protected override void OnConstructionComplete()
    {
        base.OnConstructionComplete();
        Debug.Log("Barracks completato!");
    }
}
