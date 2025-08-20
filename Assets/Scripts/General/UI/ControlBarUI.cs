using UnityEngine;
using UnityEngine.UI;

public class ControlBarUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform pointer;
    public Image leftFill;  // barra sinistra - giocatori
    public Image rightFill; // barra destra - nemici

    [Header("Settings")]
    public float barWidth = 300f;

    public void UpdateBar(float progress)
    {
        progress = Mathf.Clamp(progress, -1f, 1f);

        if (pointer != null)
        {
            pointer.anchoredPosition = new Vector2(progress * (barWidth / 2f), pointer.anchoredPosition.y);
        }

        if (leftFill != null)
        {
            float leftAmount = 0.5f + Mathf.Clamp(progress, -1f, 1f) * 0.5f;
            leftFill.fillAmount = Mathf.Clamp01(leftAmount);
        }

        if (rightFill != null)
        {
            float rightAmount = 0.5f + Mathf.Clamp(-progress, -1f, 1f) * 0.5f;
            rightFill.fillAmount = Mathf.Clamp01(rightAmount);
        }
    }
}
