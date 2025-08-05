using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    public float speed = 15f;
    public float damage = 15f;
    public GameObject impactEffect;

    [HideInInspector] public GameObject targetUnit;
    private Vector3 velocity;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float flightTime;
    private float elapsedTime = 0f;
    private MilitaryUnit shooter;

    private readonly Vector3 gravity = new Vector3(0, -9.81f, 0);

    public void Launch(Vector3 targetPos, float dmg, MilitaryUnit shooterUnit)
    {
        shooter = shooterUnit;
        damage = dmg;
        startPosition = transform.position;
        targetPosition = targetPos;

        Vector3 toTarget = targetPosition - startPosition;
        float distanceXZ = new Vector2(toTarget.x, toTarget.z).magnitude;
        float heightDifference = toTarget.y;

        float angle = 30f * Mathf.Deg2Rad; // Angolo in radianti

        float v0Squared = (Physics.gravity.magnitude * distanceXZ * distanceXZ) /
                          (2 * (distanceXZ * Mathf.Tan(angle) - heightDifference) * Mathf.Pow(Mathf.Cos(angle), 2));

        float v0 = Mathf.Sqrt(Mathf.Abs(v0Squared));
        Vector3 dirXZ = new Vector3(toTarget.x, 0, toTarget.z).normalized;

        Vector3 v0Vector = Quaternion.LookRotation(dirXZ) * Quaternion.Euler(-Mathf.Rad2Deg * angle, 0, 0) * Vector3.forward * v0;
        velocity = v0Vector;

        flightTime = distanceXZ / (v0 * Mathf.Cos(angle));
        elapsedTime = 0f;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        Vector3 displacement = velocity * elapsedTime + 0.5f * gravity * elapsedTime * elapsedTime;
        transform.position = startPosition + displacement;

        Vector3 currentVelocity = velocity + gravity * elapsedTime;
        transform.rotation = Quaternion.LookRotation(currentVelocity) * Quaternion.Euler(-90f, 0f, 0f);

        if (elapsedTime >= flightTime || transform.position.y < 0)
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
