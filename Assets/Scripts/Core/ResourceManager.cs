using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("Gather Rates per Citizen")]
    public float goldGatherRatePerCitizen = 10f; // oro al secondo per cittadino
    public float foodGatherRatePerCitizen = 10f; // cibo al secondo per cittadino

    public event System.Action<int> OnGoldChanged;
    public event System.Action<int> OnFoodChanged;

    private int _gold = 100;
    private int _food = 100;
    private float goldTimer = 0f;
    private float foodTimer = 0f;
    public float gatherInterval = 2f; // ogni 2 secondi

    public int citizensOnGold = 0;
    public int citizensOnFood = 0;
    public int citizensIdle = 0;
    public event System.Action<int> OnIdleCitizensChanged;
    public event System.Action OnWorkerDistributionChanged;

    private void Awake()
    {
        Instance = this;
    }

    public int Gold
    {
        get => _gold;
        private set
        {
            _gold = value;
            OnGoldChanged?.Invoke(_gold);
        }
    }

    public int Food
    {
        get => _food;
        private set
        {
            _food = value;
            OnFoodChanged?.Invoke(_food);
        }
    }

    public bool SpendResources(UnitType type)
    {
        switch (type)
        {
            case UnitType.Citizen:
                if (Food >= 10)
                {
                    Food -= 10;
                    return true;
                }
                break;

            case UnitType.Soldier:
                if (Gold >= 20 && Food >= 5)
                {
                    Gold -= 20;
                    Food -= 5;
                    return true;
                }
                break;

            case UnitType.Pike:
                if (Gold >= 20 && Food >= 5)
                {
                    Gold -= 20;
                    Food -= 5;
                    return true;
                }
                break;
        }

        return false;
    }

    public void UpdateCitizenCount(int count)
    {
        // Placeholder: eventualmente puoi usarlo per notificare la UI dei cittadini
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // Timer per cittadini assegnati all'oro
        goldTimer += deltaTime;
        if (goldTimer >= gatherInterval)
        {
            int goldToAdd = Mathf.RoundToInt(citizensOnGold * goldGatherRatePerCitizen);
            if (goldToAdd > 0)
                Gold += goldToAdd;

            goldTimer -= gatherInterval;
        }

        // Timer per cittadini assegnati al cibo
        foodTimer += deltaTime;
        if (foodTimer >= gatherInterval)
        {
            int foodToAdd = Mathf.RoundToInt(citizensOnFood * foodGatherRatePerCitizen);
            if (foodToAdd > 0)
                Food += foodToAdd;

            foodTimer -= gatherInterval;
        }
    }

    public void AssignCitizens(int goldWorkers, int foodWorkers)
    {
        citizensOnGold = goldWorkers;
        citizensOnFood = foodWorkers;
    }

    public void AddIdleCitizen(int amount = 1)
    {
        citizensIdle += amount;
        OnIdleCitizensChanged?.Invoke(citizensIdle);
        OnWorkerDistributionChanged?.Invoke();
    }

    public void RemoveIdleCitizen(int amount = 1)
    {
        citizensIdle = Mathf.Max(0, citizensIdle - amount);
        OnIdleCitizensChanged?.Invoke(citizensIdle);
        OnWorkerDistributionChanged?.Invoke();
    }

    public void AssignCitizenToGold()
    {
        if (citizensIdle > 0)
        {
            citizensOnGold++;
            RemoveIdleCitizen();
            OnWorkerDistributionChanged?.Invoke();
        }
    }

    public void RemoveCitizenFromGold()
    {
        if (citizensOnGold > 0)
        {
            citizensOnGold--;
            AddIdleCitizen();
            OnWorkerDistributionChanged?.Invoke();
        }
    }

    public void AssignCitizenToFood()
    {
        if (citizensIdle > 0)
        {
            citizensOnFood++;
            RemoveIdleCitizen();
            OnWorkerDistributionChanged?.Invoke();
        }
    }

    public void RemoveCitizenFromFood()
    {
        if (citizensOnFood > 0)
        {
            citizensOnFood--;
            AddIdleCitizen();
            OnWorkerDistributionChanged?.Invoke();
        }
    }
}
