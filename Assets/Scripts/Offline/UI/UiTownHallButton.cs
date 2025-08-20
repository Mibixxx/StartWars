using UnityEngine;
using System.Linq;

public class UITownHallButton : MonoBehaviour
{
    public void OnProduceCitizenClicked()
    {
        var localResources = FusionNetworkManager.Instance.GetLocalPlayer().GetComponent<PlayerResources>();
        localResources.RPC_RequestProduceCitizen();
    }
}
