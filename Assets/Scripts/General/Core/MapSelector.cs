using Fusion;
using UnityEngine;

public class MapSelector : MonoBehaviour
{
    public static Camera SelectedCamera;

    public GameObject camera1;
    public GameObject camera2;
    public GameObject light1;
    public GameObject light2;

    void Start()
    {
        NetworkRunner runner = FusionNetworkManager.Instance.GetRunner();

        if (runner != null)
        {
            // Qui uso PlayerId: 0 = primo player che entra, 1 = secondo
            if (runner.LocalPlayer.PlayerId == 1)
            {
                camera1.SetActive(true);
                camera2.SetActive(false);
                light1.SetActive(true);
                light2.SetActive(false);

                SelectedCamera = camera1.GetComponent<Camera>();
            }
            else
            {
                camera1.SetActive(false);
                camera2.SetActive(true);
                light1.SetActive(false);
                light2.SetActive(true);

                SelectedCamera = camera2.GetComponent<Camera>();
            }
        }
        else
        {
            Debug.LogError("Runner non trovato! Assicurati che FusionNetworkManager sia attivo nella scena.");
        }
    }
}
