using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingData", menuName = "Buildings/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingName;

    [Header("Construction")]
    public float constructionTime = 5f;

    [Header("Resource Costs")]
    public int goldCost;
    public int foodCost;
    public int woodCost;
    public int stoneCost;

    [Header("Prefab")]
    public GameObject prefab;
}
