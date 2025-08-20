using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CombatArea : MonoBehaviour
{
    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var unit = other.GetComponent<MilitaryUnit_Networked>();
        if (unit != null)
        {
            unit.SetInCombatArea(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var unit = other.GetComponent<MilitaryUnit_Networked>();
        if (unit != null)
        {
            unit.SetInCombatArea(false);
        }
    }
}