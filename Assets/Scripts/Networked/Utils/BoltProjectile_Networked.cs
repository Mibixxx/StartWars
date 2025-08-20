using Fusion;
using UnityEngine;

public class BoltProjectile_Networked : NetworkBehaviour
{
    public float speed = 15f;
    public float damage = 25f;
    public GameObject impactEffect;

    [Networked] public NetworkId TargetId { get; set; }

    private Vector3 startPosition;
    private Vector3 direction;
    private float traveled = 0f;
    private float travelDistance = 0f;
    private MilitaryUnit_Networked shooter;

    public void Launch(Vector3 dir, float dmg, MilitaryUnit_Networked shooterUnit, NetworkObject target)
    {
        shooter = shooterUnit;
        damage = dmg;
        startPosition = transform.position;
        direction = dir.normalized;

        if (target != null)
        {
            TargetId = target.Id;
            travelDistance = Vector3.Distance(startPosition, target.transform.position);
        }
    }

    public override void FixedUpdateNetwork()
    {
        float step = speed * Runner.DeltaTime;
        transform.position += direction * step;
        traveled += step;

        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90f, 0f, 0f);

        if (traveled >= travelDistance)
            Impact();
    }

    private void Impact()
    {
        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        if (shooter != null && Object.HasStateAuthority && TargetId != default(NetworkId))
        {
            NetworkObject targetNetObj = Runner.FindObject(TargetId);
            if (targetNetObj != null)
            {
                MilitaryUnit_Networked targetUnit = targetNetObj.GetComponent<MilitaryUnit_Networked>();
                if (targetUnit != null)
                {
                    targetUnit.TakeDamage((int)damage);
                }
            }
        }

        Runner.Despawn(Object);
    }
}
