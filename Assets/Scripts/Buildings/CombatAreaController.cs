using UnityEngine;

public class CombatAreaController : MonoBehaviour
{
    public float controlTimeRequired = 10f;
    public float controlSpeed = 1f;

    [Range(-1f, 1f)]
    public float controlProgress = 0f;

    public LayerMask playerMask;
    public LayerMask enemyMask;

    [Header("Area di Controllo")]
    public BoxCollider combatAreaCollider;

    private void Update()
    {
        if (combatAreaCollider == null)
        {
            Debug.LogWarning("CombatAreaCollider non assegnato.");
            return;
        }

        int playerCount = CountUnitsInArea(playerMask);
        int enemyCount = CountUnitsInArea(enemyMask);

        if (playerCount > 0 && enemyCount == 0)
        {
            controlProgress += (controlSpeed / controlTimeRequired) * Time.deltaTime;
        }
        else if (enemyCount > 0 && playerCount == 0)
        {
            controlProgress -= (controlSpeed / controlTimeRequired) * Time.deltaTime;
        }

        controlProgress = Mathf.Clamp(controlProgress, -1f, 1f);
        GameManager.Instance.UpdateControlProgress(controlProgress);

        CheckVictoryCondition();
    }

    int CountUnitsInArea(LayerMask mask)
    {
        Vector3 center = combatAreaCollider.bounds.center;
        Vector3 halfExtents = combatAreaCollider.bounds.extents;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, mask);
        int count = 0;

        foreach (var hit in hits)
        {
            if (((1 << hit.gameObject.layer) & playerMask) != 0)
            {
                MilitaryUnit unit = hit.GetComponent<MilitaryUnit>();
                if (unit != null && unit.IsInCombatArea)
                    count++;
            }
            else if (((1 << hit.gameObject.layer) & enemyMask) != 0)
            {
                TestEnemyUnit enemy = hit.GetComponent<TestEnemyUnit>();
                if (enemy != null && enemy.IsInCombatArea)
                    count++;
            }
        }

        return count;
    }

    void CheckVictoryCondition()
    {
        if (controlProgress >= 1f)
        {
            GameManager.Instance.DeclareVictory(true);
        }
        else if (controlProgress <= -1f)
        {
            GameManager.Instance.DeclareVictory(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (combatAreaCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(combatAreaCollider.bounds.center, combatAreaCollider.bounds.size);
        }
    }
}
