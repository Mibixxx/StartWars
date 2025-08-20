using System.Linq;
using UnityEngine;
using Fusion;

public class UITownHallButton_Networked : MonoBehaviour
{
    public void OnProduceCitizenClicked()
    {
        var localResources = FusionNetworkManager.Instance.GetLocalPlayer()?.GetComponent<PlayerResources>(); localResources.RPC_RequestProduceCitizen();
    }
}
