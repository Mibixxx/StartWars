using UnityEngine;
using TMPro;
using Fusion;
using System.Collections;

public class HUDManager_Networked : MonoBehaviour
{
    [Header("Resource Texts")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;

    [Header("Citizen Texts")]
    public TextMeshProUGUI idleCitizensText;
    public TextMeshProUGUI goldWorkersText;
    public TextMeshProUGUI foodWorkersText;
    public TextMeshProUGUI woodWorkersText;
    public TextMeshProUGUI stoneWorkersText;

    private PlayerResources localPlayerResources;

    private void Start()
    {
        StartCoroutine(WaitForLocalPlayerResources());
    }

    private IEnumerator WaitForLocalPlayerResources()
    {
        while (localPlayerResources == null)
        {
            foreach (var pr in FindObjectsByType<PlayerResources>(FindObjectsSortMode.None))
            {
                if (pr.Object.HasInputAuthority)
                {
                    localPlayerResources = pr;
                    SubscribeToEvents(pr);
                    UpdateAllTexts();
                    Debug.Log("HUD collegata al PlayerResources locale.");
                    yield break;
                }
            }
            yield return null; // aspetta un frame
        }
    }

    private void SubscribeToEvents(PlayerResources pr)
    {
        pr.OnGoldChanged += value => goldText.text = value.ToString();
        pr.OnFoodChanged += value => foodText.text = value.ToString();
        pr.OnWoodChanged += value => woodText.text = value.ToString();
        pr.OnStoneChanged += value => stoneText.text = value.ToString();

        pr.OnIdleCitizensChanged += value => idleCitizensText.text = value.ToString();

        pr.OnWorkerDistributionChanged += () =>
        {
            goldWorkersText.text = pr.CitizensOnGold.ToString();
            foodWorkersText.text = pr.CitizensOnFood.ToString();
            woodWorkersText.text = pr.CitizensOnWood.ToString();
            stoneWorkersText.text = pr.CitizensOnStone.ToString();
        };
    }

    private void UpdateAllTexts()
    {
        goldText.text = localPlayerResources.Gold.ToString();
        foodText.text = localPlayerResources.Food.ToString();
        woodText.text = localPlayerResources.Wood.ToString();
        stoneText.text = localPlayerResources.Stone.ToString();

        idleCitizensText.text = localPlayerResources.CitizensIdle.ToString();
        goldWorkersText.text = localPlayerResources.CitizensOnGold.ToString();
        foodWorkersText.text = localPlayerResources.CitizensOnFood.ToString();
        woodWorkersText.text = localPlayerResources.CitizensOnWood.ToString();
        stoneWorkersText.text = localPlayerResources.CitizensOnStone.ToString();
    }
}
