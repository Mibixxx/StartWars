using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("Gather Rates per Citizen")]
    public float goldGatherRatePerCitizen = 10f;  // oro al secondo per cittadino
    public float foodGatherRatePerCitizen = 10f;  // cibo al secondo per cittadino
    public float woodGatherRatePerCitizen = 10f;  // legno al secondo per cittadino
    public float stoneGatherRatePerCitizen = 10f; // pietra al secondo per cittadino

    public event System.Action<int> OnGoldChanged;
    public event System.Action<int> OnFoodChanged;
    public event System.Action<int> OnWoodChanged;
    public event System.Action<int> OnStoneChanged;

    private int _gold = 1000;
    private int _food = 1000;
    private int _wood = 1000;
    private int _stone = 1000;

    private float goldTimer = 0f;
    private float foodTimer = 0f;
    private float woodTimer = 0f;
    private float stoneTimer = 0f;

    public float gatherInterval = 2f; // ogni 2 secondi

    public int citizensOnGold = 0;
    public int citizensOnFood = 0;
    public int citizensOnWood = 0;
    public int citizensOnStone = 0;
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

    public int Wood
    {
        get => _wood;
        private set
        {
            _wood = value;
            OnWoodChanged?.Invoke(_wood);
        }
    }

    public int Stone
    {
        get => _stone;
        private set
        {
            _stone = value;
            OnStoneChanged?.Invoke(_stone);
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

            case UnitType.Infantry:
                if (Gold >= 20 && Food >= 5)
                {
                    Gold -= 20;
                    Food -= 5;
                    return true;
                }
                break;

            case UnitType.Spear:
                if (Gold >= 20 && Food >= 5)
                {
                    Gold -= 20;
                    Food -= 5;
                    return true;
                }
                break;

            case UnitType.Archer:
                if (Gold >= 25 && Food >= 5)
                {
                    Gold -= 25;
                    Food -= 5;
                    return true;
                }
                break;

            case UnitType.Crossbowman:
                if (Gold >= 30 && Food >= 5)
                {
                    Gold -= 30;
                    Food -= 5;
                    return true;
                }
                break;

            case UnitType.Horseman:
                if (Gold >= 40 && Food >= 10)
                {
                    Gold -= 40;
                    Food -= 10;
                    return true;
                }
                break;

            case UnitType.Knight:
                if (Gold >= 60 && Food >= 15)
                {
                    Gold -= 60;
                    Food -= 15;
                    return true;
                }
                break;
        }

        return false;
    }

    public bool SpendResourcesForBuilding(int goldCost, int foodCost, int woodCost, int stoneCost)
    {
        if (Gold >= goldCost &&
            Food >= foodCost &&
            Wood >= woodCost &&
            Stone >= stoneCost)
        {
            Gold -= goldCost;
            Food -= foodCost;
            Wood -= woodCost;
            Stone -= stoneCost;
            return true;
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

        // Timer per cittadini assegnati al legno
        woodTimer += deltaTime;
        if (woodTimer >= gatherInterval)
        {
            int woodToAdd = Mathf.RoundToInt(citizensOnWood * woodGatherRatePerCitizen);
            if (woodToAdd > 0)
                Wood += woodToAdd;

            woodTimer -= gatherInterval;
        }

        // Timer per cittadini assegnati alla pietra
        stoneTimer += deltaTime;
        if (stoneTimer >= gatherInterval)
        {
            int stoneToAdd = Mathf.RoundToInt(citizensOnStone * stoneGatherRatePerCitizen);
            if (stoneToAdd > 0)
                Stone += stoneToAdd;

            stoneTimer -= gatherInterval;
        }
    }

    public void AssignCitizens(int goldWorkers, int foodWorkers, int woodWorkers, int stoneWorkers)
    {
        citizensOnGold = goldWorkers;
        citizensOnFood = foodWorkers;
        citizensOnWood = woodWorkers;
        citizensOnStone = stoneWorkers;
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

    public void AssignCitizenToWood()
    {
        if (citizensIdle > 0)
        {
            citizensOnWood++;
            RemoveIdleCitizen();
            OnWorkerDistributionChanged?.Invoke();
        }
    }

    public void RemoveCitizenFromWood()
    {
        if (citizensOnWood > 0)
        {
            citizensOnWood--;
            AddIdleCitizen();
            OnWorkerDistributionChanged?.Invoke();
        }
    }

    public void AssignCitizenToStone()
    {
        if (citizensIdle > 0)
        {
            citizensOnStone++;
            RemoveIdleCitizen();
            OnWorkerDistributionChanged?.Invoke();
        }
    }

    public void RemoveCitizenFromStone()
    {
        if (citizensOnStone > 0)
        {
            citizensOnStone--;
            AddIdleCitizen();
            OnWorkerDistributionChanged?.Invoke();
        }
    }
}
