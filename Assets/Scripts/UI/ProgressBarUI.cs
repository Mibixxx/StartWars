using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    public Image progressFill;       // L'immagine con fillAmount
    public float duration;      // Tempo totale
    private float timer = 0f;
    private bool isRunning = false;

    public void StartProgress()
    {
        timer = 0f;
        isRunning = true;
        progressFill.fillAmount = 0f;
    }

    void Update()
    {
        if (isRunning)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            progressFill.fillAmount = progress;

            if (timer >= duration)
            {
                isRunning = false;
                Debug.Log("Completato!");
            }
        }
    }
}
