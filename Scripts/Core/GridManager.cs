using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public int gridSize = 3;
    public GameObject tilePrefab;
    public Transform gridParent;

    private void Start()
    {
        GenerateGrid();
    }

    private void Awake() => Instance = this;

    public void GenerateGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize+2; y++)
            {
                Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, gridParent);
            }
        }
    }
}