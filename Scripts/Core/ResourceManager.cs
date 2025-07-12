using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;
    public int gold = 100;
    public int food = 100;
    public int citizensOnGold = 0;
    public int citizensOnFood = 0;

    private void Awake() => Instance = this;

    public bool SpendResources(UnitType type)
    {
        switch (type)
        {
            case UnitType.Citizen:
                if (food >= 10) { food -= 10; return true; }
                break;
            case UnitType.Soldier:
                if (gold >= 20 && food >= 5) { gold -= 20; food -= 5; return true; }
                break;
        }
        return false;
    }

    public void UpdateCitizenCount(int count) { }

    private void Update()
    {
        // Produzione risorse continua
        gold += Mathf.FloorToInt(citizensOnGold * Time.deltaTime);
        food += Mathf.FloorToInt(citizensOnFood * Time.deltaTime);
    }

    public void AssignCitizens(int goldWorkers, int foodWorkers)
    {
        citizensOnGold = goldWorkers;
        citizensOnFood = foodWorkers;
    }
}