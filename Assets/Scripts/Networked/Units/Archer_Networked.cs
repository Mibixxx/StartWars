using Fusion;
using UnityEngine;

public class Archer_Networked : MilitaryUnit_Networked
{
    [Header("Archer Settings")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float fireRate = 1.5f;

    private float lastShotTime = 0f;

    public override void Spawned()
    {
        maxHP = 70;
        damage = 15;
        armor = 1;
        moveSpeed = 7f;
        attackRange = 40f;

        base.Spawned();
    }

    protected override void Attack(GameObject target)
    {
        if (Time.time >= lastShotTime + fireRate)
        {
            if (animator != null)
                animator.SetTrigger("Attack");

            lastShotTime = Time.time;
        }
    }

    public void ShootArrow()
    {
        if (!Object.HasStateAuthority) return; // solo server
        if (CurrentTarget == null || arrowPrefab == null || arrowSpawnPoint == null) return;

        Vector3 targetPos = CurrentTarget.transform.position;

        NetworkObject arrowNetObj = Runner.Spawn(
            arrowPrefab,
            arrowSpawnPoint.position,
            Quaternion.Euler(-90f, 0f, 0f)
        );

        ArrowProjectile_Networked arrow = arrowNetObj.GetComponent<ArrowProjectile_Networked>();
        if (arrow != null)
        {
            arrow.Launch(targetPos, damage, this);

            NetworkObject targetNetObj = CurrentTarget.GetComponent<NetworkObject>();
            if (targetNetObj != null)
            {
                arrow.TargetId = targetNetObj.Id;
            }
        }
    }

    public override void MoveToPosition(Vector3 position)
    {
        base.MoveToPosition(position);

        if (CurrentMode == Mode.Attacking && CurrentTarget != null)
            agent.stoppingDistance = attackRange * 0.9f;
        else
            agent.stoppingDistance = 0f;
    }
}
