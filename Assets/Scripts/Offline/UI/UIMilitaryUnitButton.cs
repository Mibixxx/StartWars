using UnityEngine;
using System.Linq;

public class UIMilitaryUnitButton : MonoBehaviour
{
    public UnitType unitType; // Assegnato via Inspector

    public void OnProduceUnitClicked()
    {
        if (!UnitManager.Instance.CanQueueUnit())
        {
            Debug.LogWarning("Limite produzione raggiunto.");
            return;
        }

        var buildings = Object.FindObjectsByType<BuildingBase>(FindObjectsSortMode.None);

        var target = buildings
            .Where(b => b.CanProduce(unitType))
            .OrderBy(b => b.GetQueue().Count)
            .FirstOrDefault();

        if (target != null)
        {
            target.ProduceUnit(unitType);
        }
        else
        {
            Debug.LogWarning($"Nessuna struttura militare disponibile per produrre: {unitType}");
        }
    }
}
