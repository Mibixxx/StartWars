using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class BuildingPulseEffect : MonoBehaviour
{
    [Header("Materials")]
    public Material pulseMaterial;      // Trasparente con effetto pulsante
    public Material finalMaterial;      // Materiale opaco normale

    [Header("Pulse Settings")]
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.8f;
    public float pulseSpeed = 2f;

    [Header("Construction State")]
    public bool isUnderConstruction = true;

    private float pulseTimer = 0f;
    private Renderer rend;
    private Material runtimeMaterial;

    void Start()
    {
        rend = GetComponent<Renderer>();

        if (isUnderConstruction && pulseMaterial != null)
        {
            // Istanza materiale trasparente per questo oggetto
            runtimeMaterial = new Material(pulseMaterial);
            rend.material = runtimeMaterial;
        }
        else if (finalMaterial != null)
        {
            rend.material = finalMaterial;
        }
    }

    void Update()
    {
        if (isUnderConstruction && runtimeMaterial != null)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(pulseTimer) + 1f) / 2f);
            runtimeMaterial.SetFloat("_AlphaValue", alpha);
        }
    }

    public void SetUnderConstruction(bool state)
    {
        isUnderConstruction = state;

        if (state)
        {
            pulseTimer = 0f;
            if (pulseMaterial != null)
                pulseMaterial.SetFloat("_AlphaValue", minAlpha);
        }
    }

    public void FinishConstruction()
    {
        isUnderConstruction = false;

        if (finalMaterial != null)
        {
            rend.material = finalMaterial;
        }

        enabled = false;
    }
}
