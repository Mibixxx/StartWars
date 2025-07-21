using UnityEngine;
using UnityEngine.UI;

public class CitizenAssignmentUI : MonoBehaviour
{
    public Button goldAddButton;
    public Button goldRemoveButton;
    public Button foodAddButton;
    public Button foodRemoveButton;

    private void Start()
    {
        goldAddButton.onClick.AddListener(() => ResourceManager.Instance.AssignCitizenToGold());
        goldRemoveButton.onClick.AddListener(() => ResourceManager.Instance.RemoveCitizenFromGold());

        foodAddButton.onClick.AddListener(() => ResourceManager.Instance.AssignCitizenToFood());
        foodRemoveButton.onClick.AddListener(() => ResourceManager.Instance.RemoveCitizenFromFood());
    }
}
