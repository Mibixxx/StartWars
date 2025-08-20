using UnityEngine;

public class GameManager_Networked : MonoBehaviour
{
    public static GameManager_Networked Instance;

    public ControlBarUI controlBarUI;

    private bool gameEnded = false;
    private CombatAreaController_Networked combatArea;

    // Lista dei texture switcher in scena
    private BannersTextureSwitcher_Networked[] bannersTextureSwitchers;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Trova automaticamente il controller (se ne hai più di uno, gestiscili diversamente)
        combatArea = FindAnyObjectByType<CombatAreaController_Networked>();

        // Trova tutti i BannersTextureSwitcher_Networked in scena
        bannersTextureSwitchers = FindObjectsByType<BannersTextureSwitcher_Networked>(FindObjectsSortMode.None);
    }

    private void Update()
    {
        // Assicurati che il controller sia valido
        if (combatArea == null)
        {
            combatArea = FindAnyObjectByType<CombatAreaController_Networked>();
            return;
        }

        if (!combatArea.Object || !combatArea.Object.IsValid) return;

        // Leggi il progresso networked
        float progress = combatArea.controlProgress;

        // Aggiorna la barra UI
        if (controlBarUI != null)
            controlBarUI.UpdateBar(progress);

        // Aggiorna tutti i texture switcher
        if (bannersTextureSwitchers != null)
        {
            foreach (var ts in bannersTextureSwitchers)
            {
                if (ts != null)
                    ts.SetControlProgress(progress);
            }
        }
    }

    public void DeclareVictory(bool playerWon)
    {
        if (gameEnded) return;
        gameEnded = true;

        if (playerWon)
            Debug.Log("Il giocatore ha conquistato la zona!");
        else
            Debug.Log("Il nemico ha conquistato la zona!");
    }
}
