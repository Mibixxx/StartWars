using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingBase : MonoBehaviour
{
    public string BuildingName;
    public bool IsBusy { get; protected set; }

    // Coda di produzione locale
    protected Queue<UnitType> localQueue = new Queue<UnitType>();
    protected const int MaxLocalQueue = 5;

    public abstract bool CanProduce(UnitType type);
    public abstract void ProduceUnit(UnitType type);

    public bool HasRoomInQueue() => localQueue.Count < MaxLocalQueue;

    public void SetBusy(bool busy)
    {
        IsBusy = busy;
    }

    public Queue<UnitType> GetQueue() => localQueue;
}
