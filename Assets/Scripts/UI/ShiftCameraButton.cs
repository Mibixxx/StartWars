using UnityEngine;
using System.Collections;
using Fusion;

public class ShiftCameraButton : MonoBehaviour
{
    private Camera targetCamera;
    public float transitionDuration = 0.5f;

    private bool isCombatView = false;
    private float z1;
    private float z2;
    private Coroutine currentTransition;

    void Start()
    {
        NetworkRunner runner = FusionNetworkManager.Instance.GetRunner();

        if (runner != null)
        {
            if (runner.LocalPlayer.PlayerId == 1)
            {
                z1 = -90.61f;
                z2 = -9.61f;
            }
            else
            {
                z1 = 191.61f;
                z2 = 101.61f;
            }
        }
        else
        {
            Debug.LogError("Runner non trovato! Assicurati che FusionNetworkManager sia attivo nella scena.");
        }
    }

    public void ToggleCameraPosition()
    {
        if (targetCamera == null)
            targetCamera = MapSelector.SelectedCamera;

        float targetZ = isCombatView ? z1 : z2;
        isCombatView = !isCombatView;

        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(MoveCameraZ(targetZ));
    }

    private IEnumerator MoveCameraZ(float targetZ)
    {
        Vector3 startLocalPos = targetCamera.transform.localPosition;
        Vector3 endLocalPos = new Vector3(startLocalPos.x, startLocalPos.y, targetZ);

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            t = 1f - Mathf.Pow(1f - t, 2f);  // Ease-out quad

            targetCamera.transform.localPosition = Vector3.Lerp(startLocalPos, endLocalPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        targetCamera.transform.localPosition = endLocalPos;
    }
}
