using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingBase : MonoBehaviour
{
    public string BuildingName;
    public bool IsBusy { get; protected set; }

    protected Queue<UnitType> localQueue = new Queue<UnitType>();
    protected const int MaxLocalQueue = 5;

    [Header("Construction Settings")]
    public float constructionTime = 5f;
    public bool IsUnderConstruction { get; private set; }

    protected BuildingPulseEffect pulseEffect;
    protected ProgressBarUI progressBar;
    public GameObject progressBarCanvas;

    protected virtual void Start()
    {
        pulseEffect = GetComponentInChildren<BuildingPulseEffect>();
        progressBar = GetComponentInChildren<ProgressBarUI>();

        BeginConstruction();
    }

    public void BeginConstruction()
    {
        IsUnderConstruction = true;

        if (pulseEffect != null)
        {
            pulseEffect.enabled = true;
            pulseEffect.SetUnderConstruction(true);
        }

        if (progressBar != null)
        {
            progressBar.duration = constructionTime;
            progressBar.StartProgress();
            // Puoi anche collegarti a un evento OnComplete se lo aggiungi in ProgressBarUI
            StartCoroutine(WaitForConstructionCompletion());
        }
        else
        {
            Debug.LogWarning("ProgressBarUI non trovata nel prefab.");
        }
    }

    private System.Collections.IEnumerator WaitForConstructionCompletion()
    {
        yield return new WaitForSeconds(constructionTime);

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
