using UnityEngine;

public class BannersTextureSwitcher_Networked : MonoBehaviour
{
    [Header("Textures")]
    public Texture neutralTexture;
    public Texture player1Texture;
    public Texture player2Texture;

    private Renderer objRenderer;
    private int lastState = 0; // -1 = player1, 0 = neutral, 1 = player2

    private void Awake()
    {
        objRenderer = GetComponentInChildren<Renderer>();

        if (objRenderer == null)
            Debug.LogError("BannersTextureSwitcher_Networked: Nessun Renderer trovato su " + gameObject.name);

        // Imposta la texture di default
        if (neutralTexture != null && objRenderer != null)
        {
            objRenderer.material.SetTexture("_BaseMap", neutralTexture);
            lastState = 0;
        }
    }

    public void SetControlProgress(float progress)
    {
        if (objRenderer == null) return;

        int currentState = 0;

        if (progress < 0f) currentState = -1;
        else if (progress > 0f) currentState = 1;

        // Aggiorna la texture solo se lo stato cambia
        if (currentState != lastState)
        {
            Texture targetTexture = neutralTexture;

            if (currentState == -1) targetTexture = player1Texture;
            else if (currentState == 1) targetTexture = player2Texture;

            objRenderer.material.SetTexture("_BaseMap", targetTexture);
            lastState = currentState;
        }
    }
}
