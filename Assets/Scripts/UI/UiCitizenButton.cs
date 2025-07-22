using UnityEngine;
using System.Linq;

public class UICitizenButton : MonoBehaviour
{
    public void OnProduceCitizenClicked()
    {
        if (!UnitManager.Instance.CanQueueUnit())
        {
            Debug.LogWarning("Limite produzione raggiunto.");
            return;
        }

        var buildings = Object.FindObjectsByType<CivilBuilding>(FindObjectsSortMode.None);

        var target = buildings
            .Where(b => b.CanProduce(UnitType.Citizen))
            .OrderBy(b => b.GetQueue().Count)
            .FirstOrDefault();

        if (target != null)
        {
            target.ProduceUnit(UnitType.Citizen);
        }
        else
        {
            Debug.LogWarning("Nessuna struttura civile disponibile per la produzione.");
        }
    }
}
