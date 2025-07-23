using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject civilBuildingPrefab;
    public GameObject militaryBuildingPrefab;
    public ControlBarUI controlBarUI;

    private bool gameEnded = false;

    // Valore tra -1 e 1 (usato dalla UI)
    public float currentControlProgress = 0f;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateControlProgress(float progress)
    {
        if (controlBarUI != null)
            controlBarUI.UpdateBar(progress);
    }

    public void DeclareVictory(bool playerWon)
    {
        if (gameEnded) return;

        gameEnded = true;

        if (playerWon)
        {
            Debug.Log("Il giocatore ha conquistato la zona!");
        }
        else
        {
            Debug.Log("Il nemico ha conquistato la zona!");
        }

        // Qui potresti bloccare l’input o mostrare una schermata di fine gioco
    }
}
