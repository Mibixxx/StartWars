using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;
    private Transform target;
    private Vector3 offset = new Vector3(0, 2f, 0);

    public void Initialize(Transform targetTransform)
    {
        target = targetTransform;
    }

    void Update()
    {
        if (target != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
    }

    public void SetHealth(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }
}
