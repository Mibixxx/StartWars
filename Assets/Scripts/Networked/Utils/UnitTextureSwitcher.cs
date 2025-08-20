using UnityEngine;
using Fusion;

public class UnitTextureSwitcher_Networked : NetworkBehaviour
{
    [Header("Textures")]
    public Texture neutralTexture;   // Texture di default
    public Texture player1Texture;   // Texture per Player1
    public Texture player2Texture;   // Texture per Player2

    private Renderer objRenderer;

    public PlayerRef OwnerPlayer;

    private void Awake()
    {
        objRenderer = GetComponentInChildren<Renderer>();
        if (objRenderer == null)
            Debug.LogError("UnitTextureSwitcher_Networked: Nessun Renderer trovato su " + gameObject.name);

        // Imposta texture di default
        if (neutralTexture != null && objRenderer != null)
            objRenderer.material.SetTexture("_BaseMap", neutralTexture);
    }

    public override void Spawned()
    {
        base.Spawned();

        // Imposta il PlayerRef owner
        OwnerPlayer = Object.InputAuthority;

        ApplyPlayerTexture();
    }

    private void ApplyPlayerTexture()
    {
        Texture textureToApply = neutralTexture;

        if (OwnerPlayer == PlayerRef.None)
        {
            textureToApply = neutralTexture;
        }
        else if (OwnerPlayer.PlayerId == 1) // Player1
        {
            textureToApply = player1Texture;
        }
        else // Player2
        {
            textureToApply = player2Texture;
        }

        if (objRenderer != null)
        {
            foreach (var mat in objRenderer.materials)
            {
                mat.SetTexture("_BaseMap", textureToApply);
            }
        }
    }
}
