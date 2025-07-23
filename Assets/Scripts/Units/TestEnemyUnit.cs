using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class TestEnemyUnit : MonoBehaviour
{
    public Collider combatArea;

    public int maxHP = 100;
    public int currentHP;
    public float moveSpeed = 1f;
    public float attackRange = 0.2f;
    public float attackCooldown = 1.2f;
    public int damage = 15;

    public Animator animator;
    public LayerMask playerUnitLayer;

    public bool IsInCombatArea { get; private set; } = false;
    private bool isDead = false;
    private float attackTimer = 0f;
    private GameObject currentTarget;
    private NavMeshAgent agent;

    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float stuckThreshold = 0.1f; // movimento minimo
    private float stuckDuration = 1.5f; 

    // Shared target assignment system
    public static Dictionary<GameObject, int> TargetAssignments = new Dictionary<GameObject, int>();

    public event System.Action<TestEnemyUnit> OnDeath;

    void Start()
    {
        lastPosition = transform.position;
        currentHP = maxHP;

        if (animator == null)
            animator = GetComponent<Animator>();

        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    void Update()
    {
        if (isDead || !IsInCombatArea) return;

        attackTimer -= Time.deltaTime;

        if (currentTarget == null || !IsTargetValid(currentTarget))
        {
            GameObject previousTarget = currentTarget;
            currentTarget = FindBestTarget();

            // Aggiorna assegnazioni
            if (previousTarget != null && TargetAssignments.ContainsKey(previousTarget))
            {
                TargetAssignments[previousTarget]--;
                if (TargetAssignments[previousTarget] <= 0)
                    TargetAssignments.Remove(previousTarget);
            }

            if (currentTarget != null)
            {
                if (TargetAssignments.ContainsKey(currentTarget))
                    TargetAssignments[currentTarget]++;
                else
                    TargetAssignments[currentTarget] = 1;
            }
        }

        if (currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (dist <= attackRange)
            {
                if (agent.enabled && agent.isOnNavMesh)
                    agent.ResetPath();

                transform.LookAt(currentTarget.transform);

                if (attackTimer <= 0f)
                {
                    Attack(currentTarget);
                    attackTimer = attackCooldown;
                }
            }
            else
            {
                if (agent.enabled && agent.isOnNavMesh)
                    agent.SetDestination(currentTarget.transform.position);
            }
        }

        if (animator != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }

        if (currentTarget != null && agent.hasPath && !agent.pathPending)
        {
            float movement = Vector3.Distance(transform.position, lastPosition);

            if (movement < stuckThreshold)
            {
                stuckTimer += Time.deltaTime;

                if (stuckTimer >= stuckDuration)
                {
                    // Riassegna bersaglio
                    if (TargetAssignments.ContainsKey(currentTarget))
                    {
                        TargetAssignments[currentTarget]--;
                        if (TargetAssignments[currentTarget] <= 0)
                            TargetAssignments.Remove(currentTarget);
                    }

                    currentTarget = FindBestTarget(); // oppure FindBestTarget() se hai un sistema avanzato
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }

            lastPosition = transform.position;
        }
    }

    public void SetInCombatArea(bool value)
    {
        IsInCombatArea = value;
    }

    GameObject FindBestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 15f, playerUnitLayer);
        GameObject bestTarget = null;
        float bestScore = float.MinValue;

        foreach (var col in hits)
        {
            MilitaryUnit unit = col.GetComponent<MilitaryUnit>();
            if (unit == null || !unit.IsInCombatArea || unit.GetComponent<Collider>() == null)
                continue;

            float distance = Vector3.Distance(transform.position, unit.transform.position);
            float vulnerabilityScore = 1f / (unit.currentHP + 1);
            float proximityScore = 1f / (distance + 0.1f);
            int assignedCount = TargetAssignments.ContainsKey(unit.gameObject) ? TargetAssignments[unit.gameObject] : 0;
            float assignmentPenalty = 1f / (assignedCount + 1);

            float totalScore = vulnerabilityScore * 0.3f + proximityScore * 0.5f + assignmentPenalty * 0.2f;

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestTarget = unit.gameObject;
            }
        }

        return bestTarget;
    }

    bool IsTargetValid(GameObject target)
    {
        if (target == null) return false;
        return target.activeInHierarchy;
    }

    void Attack(GameObject target)
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        currentTarget = target;
    }

    public void ApplyDamage()
    {
        if (currentTarget == null) return;

        MilitaryUnit unit = currentTarget.GetComponent<MilitaryUnit>();
        if (unit != null)
        {
            unit.currentHP -= damage;
            if (unit.currentHP <= 0)
                unit.Die();
        }
    }

    public void MoveToRallyPoint(Vector3 rallyPoint)
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(rallyPoint);
        }
    }

    public void SetFacingDirection(Vector3 dir)
    {
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = targetRotation;
        }
    }

    public virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null)
        {
            // animator.SetTrigger("Die");
        }

        // Rimuovi assegnazione
        if (currentTarget != null && TargetAssignments.ContainsKey(currentTarget))
        {
            TargetAssignments[currentTarget]--;
            if (TargetAssignments[currentTarget] <= 0)
                TargetAssignments.Remove(currentTarget);
        }

        agent.enabled = false;
        RallyPointManager.ReleasePosition(transform.position);
        Destroy(gameObject);
        OnDeath?.Invoke(this);
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying && currentTarget != null)
        {
            Gizmos.color = Color.red;
            Vector3 offset = Vector3.up * 0.5f;
            Gizmos.DrawLine(transform.position + offset, currentTarget.transform.position + offset);
            Gizmos.DrawSphere(currentTarget.transform.position + offset, 0.1f);
        }
    }
}
