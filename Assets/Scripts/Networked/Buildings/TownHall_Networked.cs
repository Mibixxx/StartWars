using System.Collections.Generic;
using UnityEngine;

public class TownHall_Networked : BuildingBase_Networked
{
    #region UNIT PRODUCTION

    public override bool CanProduce(UnitType type)
    {
        if (IsUnderConstruction) return false;
        if (type != UnitType.Citizen) return false;      // TownHall produce solo cittadini
        if (!HasRoomInQueue()) return false;
        if (UnitManager_Networked.Instance == null) return false;

        // Controllo capacità totale lato server
        return UnitManager_Networked.Instance.CanAddUnit();
    }

    public override void ProduceUnit(UnitType type)
    {
        if (!CanProduce(type)) return;

        if (ownerResources == null)
        {
            Debug.LogWarning("Owner PlayerResources non assegnato al TownHall!");
            return;
        }

        // Scala risorse lato server
        if (!ownerResources.SpendResources(type))
        {
            Debug.Log("Risorse insufficienti per produrre " + type);
            return;
        }

        // Aggiunge alla coda locale e notifica UnitManager_Networked
        localQueue.Enqueue(type);
        UnitManager_Networked.Instance.EnqueueProduction(this, type);

        Debug.Log($"Unità {type} aggiunta alla coda del TownHall.");
    }

    #endregion

    #region CONSTRUCTION

    protected override void OnConstructionComplete()
    {
        base.OnConstructionComplete();
        Debug.Log("TownHall completata! Produzione unità abilitata.");
    }

    #endregion

    #region UNIT COMPLETION (Server)

    // Metodo per completare la produzione della prossima unità
    public void CompleteNextUnit()
    {
        if (localQueue.Count == 0) return;

        UnitType type = localQueue.Dequeue();

        if (type == UnitType.Citizen)
        {
            UnitManager_Networked.Instance.SpawnUnit(this, type, ownerResources);
        }
        else
        {
            Debug.LogWarning("TownHall può produrre solo cittadini!");
        }
    }

    #endregion
}
