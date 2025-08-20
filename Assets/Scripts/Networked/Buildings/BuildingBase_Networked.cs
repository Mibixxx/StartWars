using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public abstract class BuildingBase_Networked : NetworkBehaviour
{
    [Networked] public bool IsUnderConstruction { get; private set; }
    [Networked] public bool IsBusy { get; private set; }

    public BuildingData buildingData;
    public Transform spawnPoint;

    protected Queue<UnitType> localQueue = new Queue<UnitType>();
    protected const int MaxLocalQueue = 5;

    [Networked]
    public PlayerResources ownerResources { get; set; }
    public PlayerResources OwnerResources => ownerResources;
    protected BuildingPulseEffect pulseEffect;
    protected ProgressBarUI progressBar;
    public GameObject progressBarCanvas;

    #region UNITY

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
            Debug.LogWarning($"BuildingData non assegnato a {gameObject.name}. Fallback a 5s.");
            BeginConstruction(5f);
        }
    }

    #endregion

    #region OWNER

    public void SetOwner(PlayerResources playerResources)
    {
        if (Runner.IsServer)
            ownerResources = playerResources;
    }

    #endregion

    #region CONSTRUCTION

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
            StartCoroutine(WaitForConstructionCompletion(duration));
        }

        if (progressBarCanvas != null)
            progressBarCanvas.SetActive(true);
    }

    private IEnumerator WaitForConstructionCompletion(float duration)
    {
        yield return new WaitForSeconds(duration);

        IsUnderConstruction = false;

        if (pulseEffect != null)
            pulseEffect.FinishConstruction();

        if (progressBarCanvas != null)
            progressBarCanvas.SetActive(false);

        OnConstructionComplete();
    }

    protected virtual void OnConstructionComplete()
    {
        Debug.Log($"{buildingData.buildingName} construction complete.");
    }

    #endregion

    #region UNIT PRODUCTION

    public bool HasRoomInQueue() => localQueue.Count < MaxLocalQueue;

    public void SetBusy(bool busy)
    {
        IsBusy = busy;
    }

    public Queue<UnitType> GetQueue() => localQueue;

    public abstract bool CanProduce(UnitType type);

    public virtual void ProduceUnit(UnitType type)
    {
        if (!CanProduce(type)) return;

        if (ownerResources == null)
        {
            Debug.LogWarning("Owner PlayerResources non assegnato!");
            return;
        }

        localQueue.Enqueue(type);

        // Notifica il UnitManager networked
        if (UnitManager_Networked.Instance != null)
            UnitManager_Networked.Instance.EnqueueProduction(this, type);
    }

    #endregion
}
