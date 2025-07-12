using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public TMPro.TextMeshProUGUI goldText;
    public TMPro.TextMeshProUGUI foodText;
    public TMPro.TextMeshProUGUI unitCountText;
    public TMPro.TMP_InputField goldInput;
    public TMPro.TMP_InputField foodInput;

    private void Update()
    {
        goldText.text = $"Gold: {ResourceManager.Instance.gold}";
        foodText.text = $"Food: {ResourceManager.Instance.food}";
        unitCountText.text = $"Units: {UnitManager.Instance.TotalUnits}/50\nQueue: {UnitManager.Instance.QueueCount}/15";
    }

    public void AssignCitizensToResources()
    {
        int g = int.Parse(goldInput.text);
        int f = int.Parse(foodInput.text);
        int total = g + f;

        if (total <= UnitManager.Instance.GetCitizenCount())
        {
            ResourceManager.Instance.AssignCitizens(g, f);
        }
    }

    public void TriggerAttack() => UnitManager.Instance.CommandAllSoldiersToAttack();
}