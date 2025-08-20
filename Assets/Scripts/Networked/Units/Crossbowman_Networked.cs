using Fusion;
using UnityEngine;

public class Crossbowman_Networked : MilitaryUnit_Networked
{
    [Header("Crossbowman Settings")]
    public GameObject boltPrefab;
    public Transform boltSpawnPoint;
    public float fireRate = 2f;

    private float lastShotTime = 0f;

    public override void Spawned()
    {
        maxHP = 80;
        damage = 25;
        armor = 2;
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

    public void ShootBolt()
    {
        if (!Object.HasStateAuthority) return; // solo server
        if (CurrentTarget == null || boltPrefab == null || boltSpawnPoint == null) return;

        Vector3 targetPos = CurrentTarget.transform.position;
        Vector3 dir = (targetPos - boltSpawnPoint.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir) * Quaternion.Euler(-90f, 0f, 0f);

        NetworkObject boltNetObj = Runner.Spawn(boltPrefab, boltSpawnPoint.position, rot);
        if (boltNetObj == null) return;

        BoltProjectile_Networked bolt = boltNetObj.GetComponent<BoltProjectile_Networked>();
        if (bolt != null)
        {
            NetworkObject targetNetObj = CurrentTarget.GetComponent<NetworkObject>();
            if (bolt != null && targetNetObj != null)
            {
                bolt.Launch(dir, damage, this, targetNetObj); // <-- ora è NetworkObject
                bolt.TargetId = targetNetObj.Id;
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
