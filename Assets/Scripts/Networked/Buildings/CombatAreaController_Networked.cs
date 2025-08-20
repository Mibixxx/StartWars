using UnityEngine;
using Fusion;
using System.Linq;

public class CombatAreaController_Networked : NetworkBehaviour
{
    [Header("Control Settings")]
    public float controlTimeRequired = 10f;
    public float controlSpeed = 1f;

    [Networked, Range(-1f, 1f)]
    public float controlProgress { get; private set; } = 0f;

    [Header("Area di Controllo")]
    public BoxCollider combatAreaCollider;

    private void Update()
    {
        if (!HasStateAuthority) return; // Solo lo StateAuthority aggiorna

        if (combatAreaCollider == null)
        {
            Debug.LogWarning("CombatAreaCollider non assegnato.");
            return;
        }

        // Conta unità dei due player
        PlayerRef player1 = Runner.ActivePlayers.First();
        PlayerRef player2 = Runner.ActivePlayers.FirstOrDefault(p => p != player1);

        int player1Count = CountUnitsInArea(player1);
        int player2Count = CountUnitsInArea(player2);

        // Aggiorna progresso
        if (player1Count > 0 && player2Count == 0)
        {
            controlProgress += (controlSpeed / controlTimeRequired) * Time.deltaTime;
        }
        else if (player2Count > 0 && player1Count == 0)
        {
            controlProgress -= (controlSpeed / controlTimeRequired) * Time.deltaTime;
        }

        controlProgress = Mathf.Clamp(controlProgress, -1f, 1f);

        CheckVictoryCondition();
    }

    private int CountUnitsInArea(PlayerRef ownerRef)
    {
        Vector3 center = combatAreaCollider.bounds.center;
        Vector3 halfExtents = combatAreaCollider.bounds.extents;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity);

        int count = 0;
        foreach (var hit in hits)
        {
            var unit = hit.GetComponent<MilitaryUnit_Networked>();
            if (unit == null) continue;

            if (!unit.Object || !unit.Object.IsValid) continue;
            if (unit.isActiveAndEnabled == false) continue;
            if (unit.CurrentHP <= 0) continue; // morto
            if (unit.RefOwner != ownerRef) continue;

            if (unit.IsInCombatArea)
                count++;
        }

        return count;
    }

    private void CheckVictoryCondition()
    {
        if (controlProgress >= 1f)
        {
            GameManager_Networked.Instance.DeclareVictory(true);  // Player1 vince
        }
        else if (controlProgress <= -1f)
        {
            GameManager_Networked.Instance.DeclareVictory(false); // Player2 vince
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
