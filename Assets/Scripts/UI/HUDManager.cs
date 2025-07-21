using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI unitCountText;
    public TextMeshProUGUI citizenCountText;

    public TextMeshProUGUI idleCitizensText;
    public TextMeshProUGUI goldWorkersText;
    public TextMeshProUGUI foodWorkersText;

    private void Start()
    {
        UnitManager.Instance.OnCitizenCountChanged += UpdateCitizenCountText;
        ResourceManager.Instance.OnGoldChanged += UpdateGoldText;
        ResourceManager.Instance.OnFoodChanged += UpdateFoodText;
        ResourceManager.Instance.OnIdleCitizensChanged += UpdateIdleCitizensText;
        ResourceManager.Instance.OnWorkerDistributionChanged += UpdateWorkerTexts;

        // Inizializza tutti i valori
        UpdateCitizenCountText(UnitManager.Instance.GetCitizenCount());
        UpdateGoldText(ResourceManager.Instance.Gold);
        UpdateFoodText(ResourceManager.Instance.Food);
        UpdateIdleCitizensText(ResourceManager.Instance.citizensIdle);
        UpdateWorkerTexts();
        UpdateUnitCountText();
    }

    private void UpdateCitizenCountText(int newCount)
    {
        citizenCountText.text = $"Cittadini: {newCount}";
        UpdateWorkerTexts(); // aggiornamento parallelo
        UpdateUnitCountText();
    }

    private void UpdateGoldText(int value)
    {
        goldText.text = $"Gold: {value}";
    }

    private void UpdateFoodText(int value)
    {
        foodText.text = $"Food: {value}";
    }

    private void UpdateIdleCitizensText(int count)
    {
        idleCitizensText.text = $"Idle: {count}";
    }

    private void UpdateWorkerTexts()
    {
        goldWorkersText.text = $"Gold: {ResourceManager.Instance.citizensOnGold}";
        foodWorkersText.text = $"Food: {ResourceManager.Instance.citizensOnFood}";
    }

    private void UpdateUnitCountText()
    {
        unitCountText.text = $"Unità totali: {UnitManager.Instance.TotalUnits}";
    }

    private void OnDestroy()
    {
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.OnCitizenCountChanged -= UpdateCitizenCountText;
            UnitManager.Instance.OnSoldierCountChanged += _ => UpdateUnitCountText();
        }

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnGoldChanged -= UpdateGoldText;
            ResourceManager.Instance.OnFoodChanged -= UpdateFoodText;
            ResourceManager.Instance.OnIdleCitizensChanged -= UpdateIdleCitizensText;
            ResourceManager.Instance.OnWorkerDistributionChanged -= UpdateWorkerTexts;
        }
    }

    public void TriggerAttack()
    {
        UnitManager.Instance.CommandAllSoldiersToAttack();
    }
}
