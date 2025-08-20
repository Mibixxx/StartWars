using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingBase : MonoBehaviour
{
    public BuildingData buildingData;
    public Transform spawnPoint;
    public string BuildingName => buildingData != null ? buildingData.buildingName : "Unnamed";

    public bool IsBusy { get; protected set; }

    protected Queue<UnitType> localQueue = new Queue<UnitType>();
    protected const int MaxLocalQueue = 5;

    public bool IsUnderConstruction { get; private set; }

    protected BuildingPulseEffect pulseEffect;
    protected ProgressBarUI progressBar;
    public GameObject progressBarCanvas;

    protected virtual void Start()
    {
        pulseEffect = GetComponentInChildren<BuildingPulseEffect>();
        progressBar = GetComponentInChildren<ProgressBarUI>();

        if (buildingData != null)
        {
            BeginConstruction(buildingData.constructionTime);
        }
        else
        {
            Debug.LogWarning("BuildingData non assegnato al prefab: " + gameObject.name);
            BeginConstruction(5f); // fallback
        }
    }

    public void BeginConstruction(float duration)
    {
        IsUnderConstruction = true;

        if (pulseEffect != null)
        {
            pulseEffect.enabled = true;
            pulseEffect.SetUnderConstruction(true);
        }

        if (progressBar != null)
        {
            progressBar.duration = duration;
            progressBar.StartProgress();
            StartCoroutine(WaitForConstructionCompletion());
        }

        if (progressBarCanvas != null)
            progressBarCanvas.SetActive(true);
    }

    private System.Collections.IEnumerator WaitForConstructionCompletion()
    {
        yield return new WaitForSeconds(buildingData != null ? buildingData.constructionTime : 5f);

        IsUnderConstruction = false;

        if (pulseEffect != null)
            pulseEffect.FinishConstruction();

        if (progressBarCanvas != null)
            progressBarCanvas.SetActive(false);

        OnConstructionComplete();
    }

    protected virtual void OnConstructionComplete()
    {
        Debug.Log($"{BuildingName} construction complete.");
    }

    public abstract bool CanProduce(UnitType type);
    public abstract void ProduceUnit(UnitType type);

    public bool HasRoomInQueue() => localQueue.Count < MaxLocalQueue;
    public void SetBusy(bool busy) => IsBusy = busy;
    public Queue<UnitType> GetQueue() => localQueue;
}
