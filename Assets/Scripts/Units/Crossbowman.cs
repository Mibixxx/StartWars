using UnityEngine;

public class Crossbowman : MilitaryUnit
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
        if (animator != null && Time.time >= lastShotTime + fireRate)
        {
            animator.SetTrigger("Attack");
            lastShotTime = Time.time;
        }
    }

    public void ShootBolt()
    {
        if (currentTarget == null) return;

        if (boltPrefab != null && boltSpawnPoint != null)
        {
            Vector3 targetPos = currentTarget.transform.position;
            Vector3 direction = (targetPos - boltSpawnPoint.position).normalized;

            Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90f, 0f, 0f);
            GameObject boltGO = Instantiate(boltPrefab, boltSpawnPoint.position, rotation);

            BoltProjectile bolt = boltGO.GetComponent<BoltProjectile>();
            if (bolt != null)
            {
                bolt.Launch(direction, damage, this, currentTarget);
            }
        }
    }

    public override void MoveToPosition(Vector3 position)
    {
        base.MoveToPosition(position);
        if (CurrentMode == Mode.Attacking && currentTarget != null)
        {
            agent.stoppingDistance = attackRange * 0.9f;
        }
        else
        {
            agent.stoppingDistance = 0f;
        }
    }
}
