using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool HasBuilding => currentBuilding != null;

    [SerializeField] private GameObject visualToHide;

    private BuildingBase currentBuilding;

    public void SetBuilding(BuildingBase newBuilding)
    {
        // Se esiste già un edificio, distruggilo
        if (currentBuilding != null)
        {
            Destroy(currentBuilding.gameObject);
        }

        currentBuilding = newBuilding;

        // Nascondi il terreno base
        if (visualToHide != null)
        {
            visualToHide.SetActive(false);
        }
    }

    public void RemoveBuilding()
    {
        if (currentBuilding != null)
        {
            Destroy(currentBuilding.gameObject);
            currentBuilding = null;
        }

        // Mostra il terreno di base
        if (visualToHide != null)
        {
            visualToHide.SetActive(true);
        }
    }

    public BuildingBase GetBuilding()
    {
        return currentBuilding;
    }
}
