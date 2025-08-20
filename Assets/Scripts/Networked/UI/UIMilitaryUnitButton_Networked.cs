using UnityEngine;
using System.Linq;
using Fusion;

public class UIMilitaryUnitButton_Networked : MonoBehaviour
{
    public UnitType unitType;

    public void OnProduceUnitClicked()
    {
        var localResources = FusionNetworkManager.Instance.GetLocalPlayer()?.GetComponent<PlayerResources>();
        if (localResources == null) return;

        localResources.RPC_RequestProduceMilitaryUnit(unitType);
    }
}
