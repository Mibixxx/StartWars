using Fusion;
using UnityEngine;

public class PlayerResources : NetworkBehaviour
{
    // Le proprietà [Networked] sono la base per la sincronizzazione dello stato in Fusion.
    // Solo il server (State Authority) può modificarle, e le modifiche vengono
    // automaticamente propagate a tutti i client.

    [Networked] public int Gold { get; set; }
    [Networked] public int Food { get; set; }
    [Networked] public int Wood { get; set; }
    [Networked] public int Stone { get; set; }

    // OnChange... callbacks possono essere aggiunti qui per aggiornare la UI quando un valore cambia, es:
    // [Networked(OnChanged = nameof(OnGoldChanged))] public int Gold { get; set; }
    // public static void OnGoldChanged(Changed<PlayerResources> changed) { ... }

    public override void Spawned()
    {
        // Questo metodo viene chiamato su server e client quando l'oggetto è stato spawnato in rete.
        // È il posto giusto per inizializzare i valori di rete.
        // Controlliamo se siamo il server prima di inizializzare per essere sicuri.
        if (Object.HasStateAuthority)
        {
            Gold = 1000;
            Food = 1000;
            Wood = 1000;
            Stone = 1000;

            Debug.Log("Risorse iniziali impostate per il giocatore.");
        }
    }

    // In futuro, qui verranno aggiunti gli RPC per la spesa delle risorse.
    // Esempio:
    // [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    // public void RPC_SpendResources(int gold, int food, int wood, int stone)
    // {
    //     if (Gold >= gold && Food >= food && Wood >= wood && Stone >= stone)
    //     {
    //         Gold -= gold;
    //         Food -= food;
    //         Wood -= wood;
    //         Stone -= stone;
    //     }
    // }

    // La logica di generazione delle risorse (che era in Update) verrebbe spostata qui,
    // dentro FixedUpdateNetwork, per essere eseguita solo sul server.
    public override void FixedUpdateNetwork()
    {
        // Eseguito solo su Server/Host
        if (GetInput(out NetworkInputData data))
        {
            // Qui andrebbe la logica di generazione risorse basata sul tempo (Runner.DeltaTime)
            // e sul numero di cittadini assegnati (che sarebbero anche loro delle variabili [Networked]).
            // Per ora, lo lasciamo vuoto come da richiesta.
        }
    }
}
