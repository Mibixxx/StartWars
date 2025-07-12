using UnityEngine;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class SoldierUnit : MonoBehaviour
{
    public enum Mode { Defending, Attacking }
    public Mode CurrentMode { get; private set; } = Mode.Defending;

    private UnityEngine.AI.NavMeshAgent agent;
    public Transform defendPoint;
    public Transform enemyVillage;

    private void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        GoToDefendPoint();
    }

    private void GoToDefendPoint()
    {
        agent.SetDestination(defendPoint.position);
    }

    public void SwitchToAttack()
    {
        CurrentMode = Mode.Attacking;
        agent.SetDestination(enemyVillage.position);
    }
}