using UnityEngine;

public class TextureSwitcher : MonoBehaviour
{
    public Texture playerTexture;     // Texture da applicare quando il player controlla
    public Texture enemyTexture;      // Texture da applicare quando il nemico controlla
    public Texture neutralTexture;    // Texture da applicare in stato neutro

    private Renderer objRenderer;

    private void Start()
    {
        objRenderer = GetComponentInChildren<Renderer>();

        if (objRenderer == null)
        {
            Debug.LogError("TextureSwitcher: Nessun Renderer trovato su " + gameObject.name);
        }
    }

    private void Update()
    {
        if (objRenderer == null || GameManager.Instance == null)
            return;

        float progress = GameManager.Instance.currentControlProgress;

        Texture targetTexture = null;

        if (progress > 0f)
        {
            targetTexture = playerTexture;
        }
        else if (progress < 0f)
        {
            targetTexture = enemyTexture;
        }
        else
        {
            targetTexture = neutralTexture;
        }

        // Cambia solo la texture principale (_MainTex) senza cambiare il materiale
        objRenderer.material.SetTexture("_BaseMap", targetTexture);
    }
}
