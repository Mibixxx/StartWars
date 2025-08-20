using UnityEngine;
using Fusion;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
public abstract class NetworkedProjectile : NetworkBehaviour
{
    public float speed = 15f;
    public float damage = 15f;
    public GameObject impactEffectPrefab;

    [Networked] public NetworkObject TargetUnit { get; set; }

    protected Vector3 startPosition;
    protected Vector3 direction;
    protected float travelDistance;
    protected float traveled = 0f;
    protected MilitaryUnit_Networked shooter;

    public virtual void Launch(Vector3 dir, float dmg, MilitaryUnit_Networked shooterUnit, NetworkObject target)
    {
        if (!Object.HasStateAuthority) return;

        shooter = shooterUnit;
        damage = dmg;
        direction = dir.normalized;
        TargetUnit = target;

        startPosition = transform.position;
        if (TargetUnit != null)
        {
            travelDistance = Vector3.Distance(startPosition, TargetUnit.transform.position);
        }
        else
        {
            travelDistance = 50f; // fallback distance
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        float step = speed * Runner.DeltaTime;
        transform.position += direction * step;
        traveled += step;

        transform.rotation = Quaternion.LookRotation(direction);

        if (traveled >= travelDistance)
        {
            Impact();
        }
    }

    protected virtual void Impact()
    {
        if (!Object.HasStateAuthority) return;

        // Spawn effetti per tutti i client
        if (impactEffectPrefab != null)
            Runner.Spawn(impactEffectPrefab, transform.position, Quaternion.identity);

        // Applica danno se target valido
        if (TargetUnit != null && shooter != null)
        {
            var targetUnitComp = TargetUnit.GetComponent<MilitaryUnit_Networked>();
            if (targetUnitComp != null)
                targetUnitComp.TakeDamage((int)damage);
        }

        Runner.Despawn(Object);
    }
}
