using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class MoveUnitsToCombatAreaButton_Networked : NetworkBehaviour
{
    public Collider combatArea;

    public void OnButtonClicked()
    {
        if (Runner == null) return;

        RPC_RequestMoveUnits();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestMoveUnits(RpcInfo info = default)
    {
        PlayerRef requestingPlayer = info.Source;
        if (requestingPlayer == PlayerRef.None && Runner != null)
            requestingPlayer = Runner.LocalPlayer;

        List<MilitaryUnit_Networked> units = MilitaryUnit_Networked.AllUnits.FindAll(u => u.RefOwner == requestingPlayer);

        Vector3 forward = combatArea.transform.forward;
        Vector3 centerPoint = combatArea.bounds.center;

        FormationManager tempManager = new FormationManager(units);
        tempManager.MoveUnitsToFormation(centerPoint, forward);

        foreach (var unit in units)
        {
            unit.SwitchToAttack();
        }
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
