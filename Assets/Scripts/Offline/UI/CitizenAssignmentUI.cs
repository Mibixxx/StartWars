using UnityEngine;
using UnityEngine.UI;

public class CitizenAssignmentUI : MonoBehaviour
{
    public Button goldAddButton;
    public Button goldRemoveButton;
    public Button foodAddButton;
    public Button foodRemoveButton;

    public Button woodAddButton;
    public Button woodRemoveButton;

    public Button stoneAddButton;
    public Button stoneRemoveButton;

    private void Start()
    {
        goldAddButton.onClick.AddListener(() => ResourceManager.Instance.AssignCitizenToGold());
        goldRemoveButton.onClick.AddListener(() => ResourceManager.Instance.RemoveCitizenFromGold());

        foodAddButton.onClick.AddListener(() => ResourceManager.Instance.AssignCitizenToFood());
        foodRemoveButton.onClick.AddListener(() => ResourceManager.Instance.RemoveCitizenFromFood());

        woodAddButton.onClick.AddListener(() => ResourceManager.Instance.AssignCitizenToWood());
        woodRemoveButton.onClick.AddListener(() => ResourceManager.Instance.RemoveCitizenFromWood());

        stoneAddButton.onClick.AddListener(() => ResourceManager.Instance.AssignCitizenToStone());
        stoneRemoveButton.onClick.AddListener(() => ResourceManager.Instance.RemoveCitizenFromStone());
    }
}
