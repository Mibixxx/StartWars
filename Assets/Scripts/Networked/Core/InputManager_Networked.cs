using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fusion;

public class InputManager_Networked : NetworkBehaviour
{
    public static InputManager_Networked Instance { get; private set; }
    public LayerMask tileLayerMask;

    public GameObject buildMenuUI;
    public GameObject townHallUI;
    public GameObject barracksUI;
    public GameObject archeryRangeUI;
    public GameObject stableUI;

    public BuildingData townHallData;
    public BuildingData barracksData;
    public BuildingData archeryRangeData;
    public BuildingData stableData;

    private Tile_Networked selectedTile;
    private PlayerResources localPlayerResources;

    public override void Spawned()
    {
        base.Spawned();

        if (Instance == null)
            Instance = this;

        if (Object.HasInputAuthority)
        {
            foreach (var kvp in FusionNetworkManager.Instance.GetAllPlayers())
            {
                var player = kvp.Value;
                if (player != null && player.Object.HasInputAuthority)
                {
                    InitializeLocalPlayerResources(player);
                    break;
                }
            }
        }
    }

    private void InitializeLocalPlayerResources(Player player)
    {
        localPlayerResources = player.GetComponent<PlayerResources>();
        if (localPlayerResources == null)
            Debug.LogError("InputManager_Networked: PlayerResources non trovato sul player locale!");

        // Ora che il player è pronto, possiamo iniziare a processare input
        enabled = true;

        // Rimuoviamo l’iscrizione all’evento
        FusionNetworkManager.Instance.OnLocalPlayerSpawned -= InitializeLocalPlayerResources;
    }

    public void SetLocalPlayerResources(PlayerResources resources)
    {
        localPlayerResources = resources;
        enabled = true;
    }

    // Disabilitiamo Update finché non abbiamo PlayerResources
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Update()
    {
        if (localPlayerResources == null)
        {
            // Proviamo a trovare il player locale usando il FusionNetworkManager
            if (FusionNetworkManager.Instance != null)
            {
                foreach (var kvp in FusionNetworkManager.Instance.GetAllPlayers())
                {
                    var player = kvp.Value;
                    if (player.Object.HasInputAuthority)
                    {
                        localPlayerResources = player.GetComponent<PlayerResources>();
                        if (localPlayerResources != null)
                        {
                            enabled = true; // ora possiamo processare input
                            Debug.Log("Player locale trovato e inizializzato.");
                        }
                        break;
                    }
                }
            }
            return; // se non l'abbiamo trovato, esci dal Update
        }

        // Logica di input normale
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            HandleTouch(Input.GetTouch(0).position);

        if (Input.GetMouseButtonDown(0))
            HandleTouch(Input.mousePosition);
    }

    private void HandleTouch(Vector2 screenPos)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayerMask))
        {
            Tile_Networked tile = hit.collider.GetComponent<Tile_Networked>();
            if (tile != null)
            {
                selectedTile = tile;

                if (!tile.HasBuilding)
                {
                    ShowBuildMenu(tile.transform.position);
                }
                else
                {
                    BuildingBase_Networked building = tile.GetBuilding();

                    if (building == null) return;

                    if (building.IsUnderConstruction)
                    {
                        Debug.Log("Edificio ancora in costruzione. Interazione bloccata.");
                        return;
                    }

                    if (building is TownHall_Networked) ShowTownHallUI(tile.transform.position);
                    else if (building is Barracks_Networked) ShowBarracksUI(tile.transform.position);
                    else if (building is ArcheryRange_Networked) ShowArcheryRangeUI(tile.transform.position);
                    else if (building is Stable_Networked) ShowStableUI(tile.transform.position);
                }
            }
        }
        else
        {
            HideAllMenus();
        }
    }

    private Vector3 ClampToScreen(Vector3 screenPosition, RectTransform uiElement)
    {
        Vector2 pivotOffset = new Vector2(uiElement.rect.width * uiElement.pivot.x, uiElement.rect.height * uiElement.pivot.y);
        Vector2 screenBounds = new Vector2(Screen.width, Screen.height);

        float clampedX = Mathf.Clamp(screenPosition.x, pivotOffset.x, screenBounds.x - (uiElement.rect.width - pivotOffset.x));
        float clampedY = Mathf.Clamp(screenPosition.y, pivotOffset.y, screenBounds.y - (uiElement.rect.height - pivotOffset.y));

        return new Vector3(clampedX, clampedY, screenPosition.z);
    }

    private void ShowUIAtPosition(GameObject uiElement, Vector3 worldPosition)
    {
        HideAllMenus();

        uiElement.SetActive(true);

        RectTransform rt = uiElement.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * 1.5f);
        uiElement.transform.position = ClampToScreen(screenPos, rt);
    }

    private void ShowBuildMenu(Vector3 position) => ShowUIAtPosition(buildMenuUI, position);
    private void ShowTownHallUI(Vector3 position) => ShowUIAtPosition(townHallUI, position);
    private void ShowBarracksUI(Vector3 position) => ShowUIAtPosition(barracksUI, position);
    private void ShowArcheryRangeUI(Vector3 position) => ShowUIAtPosition(archeryRangeUI, position);
    private void ShowStableUI(Vector3 position) => ShowUIAtPosition(stableUI, position);

    private void HideAllMenus()
    {
        buildMenuUI.SetActive(false);
        townHallUI.SetActive(false);
        barracksUI.SetActive(false);
        archeryRangeUI.SetActive(false);
        stableUI.SetActive(false);
    }

    private void BuildAtSelectedTile(BuildingData data)
    {
        if (selectedTile == null || data == null) return;

        // Prende il NetworkObject associato alla tile
        NetworkObject tileNO = selectedTile.GetComponent<NetworkObject>();
        if (tileNO == null)
        {
            Debug.LogError("La Tile selezionata non ha un NetworkObject!");
            return;
        }

        // Invia richiesta al server
        localPlayerResources.RPC_RequestBuild(tileNO, data.buildingName);

        // Chiude subito il menu lato client
        buildMenuUI.SetActive(false);
    }

    public BuildingData GetBuildingDataByName(string name)
    {
        if (townHallData.buildingName == name) return townHallData;
        if (barracksData.buildingName == name) return barracksData;
        if (archeryRangeData.buildingName == name) return archeryRangeData;
        if (stableData.buildingName == name) return stableData;
        return null;
    }

    public void BuildTownHall() => BuildAtSelectedTile(townHallData);
    public void BuildBarracks() => BuildAtSelectedTile(barracksData);
    public void BuildArcheryRange() => BuildAtSelectedTile(archeryRangeData);
    public void BuildStable() => BuildAtSelectedTile(stableData);
}
