using UnityEngine;
using System.Collections.Generic;

public class MoveUnitsToCombatAreaButton : MonoBehaviour
{
    public Collider combatArea;

    public void MoveAllUnitsToCombatArea()
    {
        foreach (MilitaryUnit unit in MilitaryUnit.AllUnits)
        {
            if (unit == null) continue;

            if (!IsInsideCombatArea(unit.transform.position))
            {
                Vector3 targetPos = GetRandomPointInsideCombatArea();
                unit.MoveToPosition(targetPos);
            }
        }
    }

    private bool IsInsideCombatArea(Vector3 position)
    {
        return combatArea.bounds.Contains(position);
    }

    private Vector3 GetRandomPointInsideCombatArea()
    {
        Bounds bounds = combatArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        float y = bounds.center.y;

        return new Vector3(x, y, z);
    }
}