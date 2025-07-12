using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private BuildingUI buildingUI;
    public GameObject civilBuildingPrefab;
    public GameObject militaryBuildingPrefab;

    private void Awake() => Instance = this;

    public void OnSelectBuilding(BuildingBase building)
    {
        buildingUI.currentBuilding = building;
    }
}
