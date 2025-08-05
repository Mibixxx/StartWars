using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;

    public TextMeshProUGUI unitCountText;

    public TextMeshProUGUI idleCitizensText;
    public TextMeshProUGUI goldWorkersText;
    public TextMeshProUGUI foodWorkersText;
    public TextMeshProUGUI woodWorkersText;
    public TextMeshProUGUI stoneWorkersText;

    private void Start()
    {
        ResourceManager.Instance.OnGoldChanged += UpdateGoldText;
        ResourceManager.Instance.OnFoodChanged += UpdateFoodText;
        ResourceManager.Instance.OnWoodChanged += UpdateWoodText;
        ResourceManager.Instance.OnStoneChanged += UpdateStoneText;

        ResourceManager.Instance.OnIdleCitizensChanged += UpdateIdleCitizensText;
        ResourceManager.Instance.OnWorkerDistributionChanged += UpdateWorkerTexts;

        UnitManager.Instance.OnCitizenCountChanged += _ => UpdateUnitCountText();
        UnitManager.Instance.OnSoldierCountChanged += _ => UpdateUnitCountText();

        // Inizializza tutti i valori
        UpdateGoldText(ResourceManager.Instance.Gold);
        UpdateFoodText(ResourceManager.Instance.Food);
        UpdateWoodText(ResourceManager.Instance.Wood);
        UpdateStoneText(ResourceManager.Instance.Stone);

        UpdateIdleCitizensText(ResourceManager.Instance.citizensIdle);
        UpdateWorkerTexts();
        UpdateUnitCountText();
    }

    private void UpdateGoldText(int value)
    {
        goldText.text = $"{value}";
    }

    private void UpdateFoodText(int value)
    {
        foodText.text = $"{value}";
    }

    private void UpdateWoodText(int value)
    {
        woodText.text = $"{value}";
    }

    private void UpdateStoneText(int value)
    {
        stoneText.text = $"{value}";
    }

    private void UpdateIdleCitizensText(int count)
    {
        idleCitizensText.text = $"{count}";
    }

    private void UpdateWorkerTexts()
    {
        goldWorkersText.text = $"{ResourceManager.Instance.citizensOnGold}";
        foodWorkersText.text = $"{ResourceManager.Instance.citizensOnFood}";
        woodWorkersText.text = $"{ResourceManager.Instance.citizensOnWood}";
        stoneWorkersText.text = $"{ResourceManager.Instance.citizensOnStone}";
    }

    private void UpdateUnitCountText()
    {
        unitCountText.text = $"{UnitManager.Instance.TotalUnits}";
    }

    private void OnDestroy()
    {
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnSoldierCountChanged += _ => UpdateUnitCountText();
        }

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnGoldChanged -= UpdateGoldText;
            ResourceManager.Instance.OnFoodChanged -= UpdateFoodText;
            ResourceManager.Instance.OnWoodChanged -= UpdateWoodText;
            ResourceManager.Instance.OnStoneChanged -= UpdateStoneText;

            ResourceManager.Instance.OnIdleCitizensChanged -= UpdateIdleCitizensText;
            ResourceManager.Instance.OnWorkerDistributionChanged -= UpdateWorkerTexts;
        }
    }

    public void TriggerAttack()
    {
        UnitManager.Instance.CommandAllSoldiersToAttack();
    }
}
