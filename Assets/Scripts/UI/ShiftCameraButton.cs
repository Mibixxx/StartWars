using UnityEngine;
using System.Collections;

public class ShiftCameraButton : MonoBehaviour
{
    public Camera targetCamera;
    public float transitionDuration = 0.5f;

    private bool isCombatView = false;
    private float z1 = -41f;
    private float z2 = 40f;
    private Coroutine currentTransition;

    public void ToggleCameraPosition()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        float targetZ = isCombatView ? z1 : z2;
        isCombatView = !isCombatView;

        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(MoveCameraZ(targetZ));
    }

    private IEnumerator MoveCameraZ(float targetZ)
    {
        Vector3 startPos = targetCamera.transform.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y, targetZ);

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;

            // Easing "ease-out": rallenta verso la fine
            t = 1f - Mathf.Pow(1f - t, 2f);  // Ease-out quad

            targetCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        targetCamera.transform.position = endPos;
    }
}
