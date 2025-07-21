using UnityEngine;

public class UICitizenButton : MonoBehaviour
{
    public void OnProduceCitizenClicked()
    {
        if (!UnitManager.Instance.CanQueueUnit())
        {
            Debug.LogWarning("Limite produzione raggiunto.");
            return;
        }

        // Non serve sapere da quale struttura proviene, ne scegliamo una qualsiasi
        var buildings = Object.FindObjectsByType<CivilBuilding>(FindObjectsSortMode.None);

        foreach (var b in buildings)
        {
            if (b.CanProduce(UnitType.Citizen))
            {
                b.ProduceUnit(UnitType.Citizen);
                return;
            }
        }

        Debug.LogWarning("Nessuna struttura civile disponibile per la produzione.");
    }
}
