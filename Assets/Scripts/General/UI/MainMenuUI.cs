using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button singlePlayerButton;
    [SerializeField] private Button multiplayerButton;

    [SerializeField] private string gameSceneName = "GameScene"; // nome della scena di gioco

    private void Start()
    {
        singlePlayerButton.onClick.AddListener(OnSinglePlayerClicked);
        multiplayerButton.onClick.AddListener(OnMultiplayerClicked);
    }

    private async void OnSinglePlayerClicked()
    {
        Debug.Log("Single Player button clicked");
        singlePlayerButton.interactable = false;
        multiplayerButton.interactable = false;

        await FusionNetworkManager.Instance.StartSinglePlayerGame(gameSceneName);
    }

    private async void OnMultiplayerClicked()
    {
        Debug.Log("Multiplayer button clicked");
        singlePlayerButton.interactable = false;
        multiplayerButton.interactable = false;

        await FusionNetworkManager.Instance.StartOnlineGame();
    }
}
