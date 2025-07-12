using UnityEngine;

public class CitizenUnit : MonoBehaviour
{
    public enum ResourceType { Gold, Food }

    public float health = 100f;
    public float maxHealth = 100f;

    public float goldGatherRate = 1f; // oro per secondo
    public float foodGatherRate = 1f; // cibo per secondo

    public ResourceType AssignedResource { get; private set; }
    public bool IsAlive => health > 0;

    public CitizenUnit(ResourceType assignedResource)
    {
        AssignedResource = assignedResource;
    }

    public void AssignTo(ResourceType newResource)
    {
        AssignedResource = newResource;
    }

    public float Gather(float deltaTime)
    {
        if (!IsAlive) return 0;

        return AssignedResource switch
        {
            ResourceType.Gold => goldGatherRate * deltaTime,
            ResourceType.Food => foodGatherRate * deltaTime,
            _ => 0
        };
    }

    public void TakeDamage(float damage)
    {
        health = Mathf.Max(0, health - damage);
    }

    public void Heal(float amount)
    {
        health = Mathf.Min(maxHealth, health + amount);
    }
}
