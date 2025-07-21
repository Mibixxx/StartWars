using UnityEngine;

public class UIProduceUnitButton : MonoBehaviour
{
    public UnitType unitType; // Impostabile da Inspector

    public void OnProduceUnitClicked()
    {
        if (!UnitManager.Instance.CanQueueUnit())
        {
            Debug.LogWarning("Limite produzione raggiunto.");
            return;
        }

        var buildings = Object.FindObjectsByType<MilitaryBuilding>(FindObjectsSortMode.None);

        foreach (var b in buildings)
        {
            if (b.CanProduce(unitType))
            {
                b.ProduceUnit(unitType);
                return;
            }
        }

        Debug.LogWarning($"Nessuna struttura militare disponibile per produrre: {unitType}");
    }
}
