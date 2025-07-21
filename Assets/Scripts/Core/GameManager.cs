using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject civilBuildingPrefab;
    public GameObject militaryBuildingPrefab;

    private void Awake()
    {
        Instance = this;
    }
}
