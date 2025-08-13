using UnityEngine;

public class Archer : MilitaryUnit
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
        attackRange = 40f; // sovrascrive il valore base

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

    public void ShootArrow()
    {
        if (currentTarget == null) return;

        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            Vector3 staticTargetPosition = currentTarget.transform.position;
            GameObject arrowGO = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));

            ArrowProjectile arrow = arrowGO.GetComponent<ArrowProjectile>();
            if (arrow != null)
            {
                arrow.Launch(staticTargetPosition, damage, this);
                arrow.targetUnit = currentTarget;
            }
        }
    }

    public override void MoveToPosition(Vector3 position)
    {
        base.MoveToPosition(position);
        if (CurrentMode == Mode.Attacking && currentTarget != null)
        {
            agent.stoppingDistance = attackRange * 0.9f; // in combattimento: resta a distanza
        }
        else
        {
            agent.stoppingDistance = 0f; // al rally point o movimenti normali
        }
    }
}
