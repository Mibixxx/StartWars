using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Fusion;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class MilitaryUnit_Networked : NetworkBehaviour
{
    public enum Mode { Defending, Attacking }

    [Networked] public Mode CurrentMode { get; private set; } = Mode.Defending;
    [Networked] public int CurrentHP { get; private set; }

    [Header("Combat Settings")]
    public Transform enemyVillage;
    public Collider combatArea;
    public float attackRange = 0.2f;
    public float attackCooldown = 1.2f;

    public float moveSpeed;
    public int maxHP;
    public int damage;
    public int armor;

    public GameObject healthBarPrefab;
    private HealthBarUI healthBarInstance;

    protected NavMeshAgent agent;
    protected Animator animator;

    private bool isDead = false;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private GameObject currentTarget;
    public GameObject CurrentTarget => currentTarget;

    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private float stuckThreshold = 1f;
    private float stuckDuration = 1.5f;
    [Networked] private Vector3 targetPosition { get; set; }

    [Networked] public PlayerRef RefOwner { get; private set; }
    [Networked] public bool IsInCombatArea { get; private set; }
    [Networked] private int attackAnimationTick { get; set; }
    private int lastProcessedTick = -1;

    public static List<MilitaryUnit_Networked> AllUnits = new List<MilitaryUnit_Networked>();
    public static Dictionary<GameObject, int> TargetAssignments = new Dictionary<GameObject, int>();

    public override void Spawned()
    {
        base.Spawned();

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = moveSpeed;

        CurrentHP = maxHP;
        AllUnits.Add(this);
        RefOwner = Object.InputAuthority;

        healthBarInstance = GetComponentInChildren<HealthBarUI>();
        if (healthBarInstance != null)
        {
            healthBarInstance.Initialize(transform);
            UpdateHealthBar();
        }

        Debug.Log($"[Spawned] {name} assegnata a {RefOwner}");
    }

    protected virtual void Update()
    {
        if (isDead) return;

        if (HasStateAuthority)
        {
            attackTimer -= Time.deltaTime;

            if (CurrentMode == Mode.Attacking && combatArea != null)
            {
                HandleCombat();
                CheckIfStuck();
            }
        }

        HandleMovementAnimation();
        SyncAttackAnimation();
    }

    private void HandleMovementAnimation()
    {
        if (animator != null && agent != null)
        {
            float speed = isAttacking ? 0f : agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
    }

    protected void HandleCombat()
    {
        if (currentTarget == null || !IsTargetValid(currentTarget))
        {
            GameObject previousTarget = currentTarget;
            MilitaryUnit_Networked newTarget = FindBestTarget();
            currentTarget = newTarget != null ? newTarget.gameObject : null;

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

        if (currentTarget != null && IsTargetValid(currentTarget))
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distance <= attackRange)
            {
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;
                }

                isAttacking = true;

                if (attackTimer <= 0f)
                {
                    Attack(currentTarget);
                    attackTimer = attackCooldown;
                }
            }
            else
            {
                if (agent != null && agent.isOnNavMesh)
                    agent.SetDestination(currentTarget.transform.position);

                isAttacking = false;
            }
        }
        else
        {
            isAttacking = false;
        }
    }

    protected virtual MilitaryUnit_Networked FindBestTarget()
    {
        MilitaryUnit_Networked bestTarget = null;
        float bestScore = float.MinValue;

        foreach (var enemy in AllUnits)
        {
            if (enemy == this || enemy.RefOwner == this.RefOwner)
                continue;

            if (enemy.isDead) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            float vulnerabilityScore = 1f / (enemy.CurrentHP + 1);
            float proximityScore = 1f / (distance + 1f);

            int assignedCount = TargetAssignments.ContainsKey(enemy.gameObject)
                ? TargetAssignments[enemy.gameObject]
                : 0;
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

    public void SetInCombatArea(bool value)
    {
        if (HasStateAuthority)
            IsInCombatArea = value;
    }

    protected virtual bool IsTargetValid(GameObject target)
    {
        if (target == null) return false;

        var unit = target.GetComponent<MilitaryUnit_Networked>();
        if (unit == null) return false;
        if (unit.isDead) return false;
        if (unit.RefOwner == this.RefOwner) return false;
        if (!unit.IsInCombatArea) return false;

        return true;
    }

    private void SyncAttackAnimation()
    {
        if (attackAnimationTick != lastProcessedTick)
        {
            int previousTick = lastProcessedTick;
            lastProcessedTick = attackAnimationTick;

            if (animator != null)
            {
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Attack");

                float tickDiff = attackAnimationTick - Runner.Tick;

                float targetSpeed = 1f + tickDiff * 0.05f;
                targetSpeed = Mathf.Max(0.01f, targetSpeed);
                animator.speed = Mathf.Lerp(animator.speed, targetSpeed, 0.2f);
            }
        }
        else
        {
            if (animator != null && Mathf.Abs(animator.speed - 1f) > 0.01f)
                animator.speed = Mathf.Lerp(animator.speed, 1f, 0.2f);
        }
    }


    protected virtual void Attack(GameObject target)
    {
        if (target == null || !IsTargetValid(target)) return;

        currentTarget = target;

        if (HasStateAuthority)
        {
            attackAnimationTick = Runner.Tick;
        }

        if (Object.HasInputAuthority && animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }
    }

    public void OnAttackAnimationHit()
    {
        if (!HasStateAuthority) return;
        if (currentTarget == null) return;

        if (!currentTarget.TryGetComponent<MilitaryUnit_Networked>(out var targetUnit))
            return;

        if (targetUnit == null || targetUnit.isDead || !targetUnit.Object || !targetUnit.Object.IsValid)
            return;

        RPC_DealDamage(targetUnit.Object, damage);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_DealDamage(NetworkObject targetObj, int amount, RpcInfo info = default)
    {
        if (targetObj == null || !targetObj || !targetObj.IsValid)
            return;

        var targetUnit = targetObj.GetComponent<MilitaryUnit_Networked>();
        if (targetUnit == null || targetUnit.isDead)
            return;

        if (!HasStateAuthority) return;

        targetUnit.TakeDamage(amount);
    }

    public void TakeDamage(int amount)
    {
        if (!HasStateAuthority) return;
        if (isDead) return;
        if (Object == null || !Object || !Object.IsValid) return;

        CurrentHP -= amount;

        if (CurrentHP <= 0)
        {
            Die();
            return;
        }

        RPC_UpdateHealthBar(CurrentHP, maxHP);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateHealthBar(int current, int max)
    {
        if (healthBarInstance != null)
            healthBarInstance.SetHealth(current, max);
    }

    private void UpdateHealthBar()
    {
        if (isDead) return;

        if (healthBarInstance != null)
            healthBarInstance.SetHealth(CurrentHP, maxHP);
    }

    public override void FixedUpdateNetwork()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        if (currentTarget != null && IsTargetValid(currentTarget))
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distance <= attackRange)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                targetPosition = transform.position;
                return;
            }
        }

        if (targetPosition != Vector3.zero)
        {
            agent.SetDestination(targetPosition);
        }
    }

    public virtual void MoveToPosition(Vector3 position)
    {
        if (!HasStateAuthority) return;
        targetPosition = position;
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
                currentTarget = null;
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
        if (!HasStateAuthority) return;
        CurrentMode = Mode.Attacking;
    }

    public virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        if (healthBarInstance != null)
            Destroy(healthBarInstance.gameObject);

        if (HasStateAuthority && UnitManager_Networked.Instance != null)
        {
            UnitManager_Networked.Instance.RemoveSoldier(this, RefOwner);
        }

        if (TargetAssignments.ContainsKey(Object.gameObject))
            TargetAssignments.Remove(Object.gameObject);

        foreach (var unit in AllUnits)
        {
            if (unit == this) continue;

            if (unit.currentTarget == this.gameObject)
            {
                unit.currentTarget = null;
                unit.isAttacking = false;
            }
        }

        AllUnits.Remove(this);

        if (HasStateAuthority)
            Runner.Despawn(Object);
    }
}
