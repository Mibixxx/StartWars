using UnityEngine;

public class BoltProjectile : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 25f;
    public GameObject impactEffect;

    [HideInInspector] public GameObject targetUnit;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 direction;
    private float travelDistance;
    private float traveled = 0f;
    private MilitaryUnit shooter;

    public void Launch(Vector3 dir, float dmg, MilitaryUnit shooterUnit, GameObject target)
    {
        shooter = shooterUnit;
        damage = dmg;
        startPosition = transform.position;
        direction = dir.normalized;
        targetUnit = target;

        if (targetUnit != null)
        {
            targetPosition = targetUnit.transform.position;
            travelDistance = Vector3.Distance(startPosition, targetPosition);
        }
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position += direction * step;
        traveled += step;

        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90f, 0f, 0f);

        if (traveled >= travelDistance)
        {
            Impact();
        }
    }

    void Impact()
    {
        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        if (targetUnit != null && shooter != null)
            shooter.ApplyDamageTo(targetUnit);

        Destroy(gameObject);
    }
}
