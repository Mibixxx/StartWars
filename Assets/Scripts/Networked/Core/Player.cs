using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Networked]
    public PlayerRef PlayerRef { get; set; }

    public PlayerResources Resources { get; private set; }

    private void Awake()
    {
        Resources = GetComponent<PlayerResources>();
    }

    public override void Spawned()
    {
        base.Spawned();

        if (HasStateAuthority)
            DontDestroyOnLoad(gameObject);

        FusionNetworkManager.Instance?.AddPlayerToList(this);

        Debug.Log($"Player object for PlayerRef {PlayerRef.PlayerId} has been spawned.");
    }

    public void OnDisconnected()
    {
        if (FusionNetworkManager.Instance != null)
        {
            FusionNetworkManager.Instance.RemovePlayerFromList(this);
        }

        if (Runner != null)
        {
            Runner.Despawn(Object);
        }
    }
}
