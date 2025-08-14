using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // Questo riferimento ci dice a quale giocatore di Fusion appartiene questo oggetto.
    // [Networked] significa che questa variabile è sincronizzata attraverso la rete.
    // OnChanged è un callback che viene chiamato quando il valore di questa variabile cambia.
    [Networked(OnChanged = nameof(OnPlayerChanged))]
    public PlayerRef PlayerRef { get; set; }

    // Riferimento al componente che gestisce le risorse di questo giocatore.
    public PlayerResources Resources { get; private set; }

    private void Awake()
    {
        // Troviamo il componente PlayerResources che si trova sullo stesso GameObject.
        Resources = GetComponent<PlayerResources>();
    }

    public override void Spawned()
    {
        // Spawned() viene chiamato da Fusion su tutti i client dopo che l'oggetto è stato creato in rete.
        // Controlliamo se questo oggetto ha l'autorità di stato (cioè, se siamo il server o l'host).
        // Questo previene che i client creino duplicati dell'oggetto tra le scene.
        if (HasStateAuthority)
        {
            // Il metodo DontDestroyOnLoad di Unity impedisce che questo GameObject venga distrutto
            // quando viene caricata una nuova scena. È fondamentale per mantenere lo stato del giocatore.
            DontDestroyOnLoad(gameObject);
        }

        // Aggiungiamo questo giocatore a un dizionario statico per un facile accesso da altre parti del codice.
        // Questo ci permette di trovare facilmente l'oggetto Player corrispondente a un PlayerRef.
        if (FusionNetworkManager.Instance != null)
        {
            FusionNetworkManager.Instance.AddPlayerToList(this);
        }

        Debug.Log($"Player object for PlayerRef {PlayerRef.PlayerId} has been spawned.");
    }

    // Questo è il callback che viene chiamato quando la proprietà PlayerRef cambia.
    private static void OnPlayerChanged(Changed<Player> changed)
    {
        // Qui potremmo inserire logica aggiuntiva se necessario, ad esempio per aggiornare la UI
        // quando il giocatore viene inizializzato.
        Debug.Log($"PlayerRef for player object changed to {changed.Behaviour.PlayerRef}");
    }

    public void OnDisconnected()
    {
        // Questo metodo viene chiamato quando un giocatore si disconnette.
        // Rimuoviamo il giocatore dalla lista e distruggiamo il suo GameObject.
        if (FusionNetworkManager.Instance != null)
        {
            FusionNetworkManager.Instance.RemovePlayerFromList(this);
        }

        if (Runner != null && Runner.IsValid)
        {
            Runner.Despawn(Object);
        }
    }
}
