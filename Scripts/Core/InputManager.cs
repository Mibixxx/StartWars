using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public LayerMask tileLayerMask;
    public GameObject buildMenuUI; // UI con bottoni per costruire
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
            if (tile != null && !tile.HasBuilding)
            {
                selectedTile = tile;
                ShowBuildMenu(tile.transform.position);
                Debug.Log($"Tile cliccata: {tile.name} in posizione {tile.transform.position}");
            }
        }
        else
        {
            Debug.Log("Nessuna tile colpita dal raycast.");
            buildMenuUI.SetActive(false);
        }
    }

    private void ShowBuildMenu(Vector3 position)
    {
        buildMenuUI.SetActive(true);
        buildMenuUI.transform.position = Camera.main.WorldToScreenPoint(position + Vector3.up * 1.5f);
    }

    private void BuildAtSelectedTile(GameObject buildingPrefab)
    {
        if (selectedTile == null) return;

        Vector3 buildPosition = selectedTile.transform.position;
        GameObject building = Instantiate(buildingPrefab, buildPosition, Quaternion.identity);
        building.transform.SetParent(selectedTile.transform);

        selectedTile.SetBuilding(building.GetComponent<BuildingBase>());
        buildMenuUI.SetActive(false);
    }


    public void BuildCivilBuilding()
    {
        BuildAtSelectedTile(GameManager.Instance.civilBuildingPrefab);
    }

    public void BuildMilitaryBuilding()
    {
        BuildAtSelectedTile(GameManager.Instance.militaryBuildingPrefab);
    }

}
