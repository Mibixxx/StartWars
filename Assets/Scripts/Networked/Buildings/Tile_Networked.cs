using Fusion;
using UnityEngine;

public class Tile_Networked : NetworkBehaviour
{
    [Networked] private NetworkObject currentBuilding { get; set; }

    [SerializeField] private GameObject visualToHide;

    public bool HasBuilding => currentBuilding != null;

    public BuildingBase_Networked GetBuilding()
    {
        return currentBuilding != null ? currentBuilding.GetComponent<BuildingBase_Networked>() : null;
    }

    public void SetBuilding(NetworkObject newBuilding)
    {
        if (currentBuilding != null)
        {
            Runner.Despawn(currentBuilding);
        }

        currentBuilding = newBuilding;

        currentBuilding.transform.position = transform.position;
        currentBuilding.transform.rotation = Quaternion.identity;

        if (visualToHide != null)
            visualToHide.SetActive(false);
    }

    public void RemoveBuilding()
    {
        if (currentBuilding != null)
        {
            Runner.Despawn(currentBuilding);
            currentBuilding = null;
        }

        if (visualToHide != null)
            visualToHide.SetActive(true);
    }
}
