using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class MilitaryUnit : MonoBehaviour
{
    public enum Mode { Defending, Attacking }
    public Mode CurrentMode { get; protected set; } = Mode.Defending;

    protected NavMeshAgent agent;
    protected Animator animator;

    [Header("Combat Settings")]
    public Transform enemyVillage;
    public Collider combatArea;
    public float attackRange = 0.2f;
    public float attackCooldown = 1.2f;
    public LayerMask enemyUnitLayer;
    public static List<MilitaryUnit> AllUnits = new List<MilitaryUnit>();
    public static Dictionary<GameObject, int> TargetAssignments = new Dictionary<GameObject, int>();

    public float moveSpeed;
    public int maxHP;
    public int currentHP;
    public int damage;
    public int armor;

    public GameObject healthBarPrefab;
    private HealthBarUI healthBarInstance;

    public bool IsInCombatArea { get; private set; } = false;
    protected bool isDead = false;
    private float attackTimer = 0f;
    protected GameObject currentTarget;

    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float stuckThreshold = 1f; // movimento minimo
    private float stuckDuration = 1.5f;

    protected virtual void Start()
    {
        AllUnits.Add(this);

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = moveSpeed;
        currentHP = maxHP;

        healthBarInstance = GetComponentInChildren<HealthBarUI>();

        if (healthBarInstance != null)
        {
            healthBarInstance.Initialize(transform);
            UpdateHealthBar();
        }
    }

    protected virtual void Update()
    {
        if (isDead) return;

        if (animator != null && agent != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }

        attackTimer -= Time.deltaTime;

        if (CurrentMode == Mode.Attacking && combatArea != null)
        {
            HandleCombat();
            CheckIfStuck();
        }
    }

    protected void HandleCombat()
    {
        if (currentTarget == null || !IsTargetValid(currentTarget))
        {
            GameObject previousTarget = currentTarget;
            currentTarget = FindBestTarget();

            // Aggiorna assegnazioni target
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
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= attackRange)
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
                MoveToPosition(currentTarget.transform.position);
            }
        }
    }

    protected virtual void Attack(GameObject target)
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        currentTarget = target;
    }

    public void ApplyDamage()
    {
        if (currentTarget == null) return;

        TestEnemyUnit enemy = currentTarget.GetComponent<TestEnemyUnit>();
        if (enemy != null)
        {
            enemy.currentHP -= damage;
            enemy.UpdateHealthBar();
            if (enemy.currentHP <= 0)
                enemy.Die();
        }
    }

    //METODO PER UNITà CON ATTACCHI A DISTANZA
    public void ApplyDamageTo(GameObject target)
    {
        if (target == null) return;

        TestEnemyUnit enemy = target.GetComponent<TestEnemyUnit>();
        if (enemy != null)
        {
            enemy.currentHP -= damage;
            enemy.UpdateHealthBar();
            if (enemy.currentHP <= 0)
                enemy.Die();
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
            Die();

        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        if (healthBarInstance != null)
        {
            healthBarInstance.SetHealth(currentHP, maxHP);
        }
    }

    public void SetInCombatArea(bool value)
    {
        IsInCombatArea = value;
    }

    protected virtual bool IsTargetValid(GameObject target)
    {
        if (target == null) return false;
        return target.activeInHierarchy;
    }

    protected virtual GameObject FindBestTarget()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, 150f, enemyUnitLayer);

        GameObject bestTarget = null;
        float bestScore = float.MinValue;

        foreach (var col in enemiesInRange)
        {
            GameObject enemy = col.gameObject;
            TestEnemyUnit enemyData = enemy.GetComponent<TestEnemyUnit>();

            if (enemyData == null || !enemyData.IsInCombatArea) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            float vulnerabilityScore = 1f / (enemyData.currentHP + 1);
            float proximityScore = 1f / (distance + 1f);
            int assignedCount = TargetAssignments.ContainsKey(enemy) ? TargetAssignments[enemy] : 0;
            float assignmentPenalty = 1f / (assignedCount + 1);

            float totalScore = vulnerabilityScore * 0.3f + proximityScore * 0.5f + assignmentPenalty * 0.2f;

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    public virtual void MoveToPosition(Vector3 position)
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        agent.SetDestination(position);
        agent.stoppingDistance = 0.3f;
        agent.avoidancePriority = Random.Range(30, 70);
    }

    private void CheckIfStuck()
    {
        if (currentTarget == null || agent == null || !agent.enabled) return;

        float movedDistance = Vector3.Distance(transform.position, lastPosition);

        if (movedDistance < stuckThreshold)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer >= stuckDuration)
            {
                if (TargetAssignments.ContainsKey(currentTarget))
                {
                    TargetAssignments[currentTarget]--;
                    if (TargetAssignments[currentTarget] <= 0)
                        TargetAssignments.Remove(currentTarget);
                }

                currentTarget = null; // Forza nuova assegnazione nel prossimo Update
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    public virtual void SwitchToAttack()
    {
        CurrentMode = Mode.Attacking;
        agent.SetDestination(enemyVillage.position);
    }

    public virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        if (healthBarInstance != null)
            Destroy(healthBarInstance.gameObject);

        if (currentTarget != null && TargetAssignments.ContainsKey(currentTarget))
        {
            TargetAssignments[currentTarget]--;
            if (TargetAssignments[currentTarget] <= 0)
                TargetAssignments.Remove(currentTarget);
        }

        UnitManager.Instance.RemoveSoldier(this);
        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        AllUnits.Remove(this);
    }

    protected virtual void OnDrawGizmos()
    {
        if (Application.isPlaying && currentTarget != null)
        {
            Vector3 offset = Vector3.up * 0.5f;
            Gizmos.DrawLine(transform.position + offset, currentTarget.transform.position + offset);
            Gizmos.DrawSphere(currentTarget.transform.position + offset, 0.1f);

        }
    }
}
