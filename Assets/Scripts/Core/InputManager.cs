using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
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

    private Tile selectedTile;

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleTouch(Input.GetTouch(0).position);
        }
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch(Input.mousePosition);
        }
#endif
    }

    private void HandleTouch(Vector2 screenPos)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayerMask))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                selectedTile = tile;

                if (!tile.HasBuilding)
                {
                    ShowBuildMenu(tile.transform.position);
                }
                else
                {
                    BuildingBase building = tile.GetBuilding();

                    if (building == null) return;

                    if (building.IsUnderConstruction)
                    {
                        Debug.Log("Edificio ancora in costruzione. Interazione bloccata.");
                        return;
                    }

                    if (building is TownHall)
                    {
                        ShowTownHallUI(tile.transform.position);
                    }
                    else if (building is Barracks)
                    {
                        ShowBarracksUI(tile.transform.position);
                    }
                    else if (building is ArcheryRange)
                    {
                        ShowArcheryRangeUI(tile.transform.position);
                    }
                    else if (building is Stable)
                    {
                        ShowStableUI(tile.transform.position);
                    }
                }
            }
        }
        else
        {
            Debug.Log("Nessuna tile colpita dal raycast.");
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

        bool success = ResourceManager.Instance.SpendResourcesForBuilding(
            data.goldCost, data.foodCost, data.woodCost, data.stoneCost
        );

        if (!success)
        {
            Debug.Log("Risorse insufficienti per costruire: " + data.buildingName);
            return;
        }

        GameObject building = Instantiate(data.prefab, selectedTile.transform.position, Quaternion.identity);
        building.transform.SetParent(selectedTile.transform);

        BuildingBase buildingBase = building.GetComponent<BuildingBase>();
        if (buildingBase != null)
        {
            buildingBase.buildingData = data;
            selectedTile.SetBuilding(buildingBase);
        }
        else
        {
            Debug.LogError("Prefab non contiene BuildingBase: " + data.buildingName);
        }

        buildMenuUI.SetActive(false);
    }

    public void BuildTownHall() => BuildAtSelectedTile(townHallData);
    public void BuildBarracks() => BuildAtSelectedTile(barracksData);
    public void BuildArcheryRange() => BuildAtSelectedTile(archeryRangeData);
    public void BuildStable() => BuildAtSelectedTile(stableData);
}
