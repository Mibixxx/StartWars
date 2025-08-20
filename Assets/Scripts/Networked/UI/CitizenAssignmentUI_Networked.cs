using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Linq;
using System.Collections;

public class CitizenAssignmentUI_Networked : MonoBehaviour
{
    public Button goldAddButton;
    public Button goldRemoveButton;
    public Button foodAddButton;
    public Button foodRemoveButton;
    public Button woodAddButton;
    public Button woodRemoveButton;
    public Button stoneAddButton;
    public Button stoneRemoveButton;

    private PlayerResources localPlayerResources;

    private IEnumerator Start()
    {
        Player localPlayer = null;

        while (localPlayer == null)
        {
            localPlayer = FindObjectsByType<Player>(FindObjectsSortMode.None)
                .FirstOrDefault(p => p.Object.HasInputAuthority);
            yield return null;
        }

        HandleLocalPlayerSpawned(localPlayer);
    }

    private void OnDestroy()
    {
        if (FusionNetworkManager.Instance != null)
            FusionNetworkManager.Instance.OnLocalPlayerSpawned -= HandleLocalPlayerSpawned;
    }

    private void HandleLocalPlayerSpawned(Player player)
    {
        localPlayerResources = player.GetComponent<PlayerResources>();
        if (localPlayerResources == null)
        {
            Debug.LogError("CitizenAssignmentUI_Networked: PlayerResources non trovato sul player locale!");
            return;
        }

        // Inizializza i listener dei bottoni
        goldAddButton.onClick.AddListener(() => SendRequest(ResourceType.Gold, true));
        goldRemoveButton.onClick.AddListener(() => SendRequest(ResourceType.Gold, false));

        foodAddButton.onClick.AddListener(() => SendRequest(ResourceType.Food, true));
        foodRemoveButton.onClick.AddListener(() => SendRequest(ResourceType.Food, false));

        woodAddButton.onClick.AddListener(() => SendRequest(ResourceType.Wood, true));
        woodRemoveButton.onClick.AddListener(() => SendRequest(ResourceType.Wood, false));

        stoneAddButton.onClick.AddListener(() => SendRequest(ResourceType.Stone, true));
        stoneRemoveButton.onClick.AddListener(() => SendRequest(ResourceType.Stone, false));
    }

    private void SendRequest(ResourceType type, bool add)
    {
        if (localPlayerResources != null)
        {
            localPlayerResources.RPC_RequestCitizenAssignment(type, add);
        }
    }
}
