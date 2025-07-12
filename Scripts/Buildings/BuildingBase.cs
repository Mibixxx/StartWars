using UnityEngine;

public abstract class BuildingBase : MonoBehaviour
{
    public string BuildingName;
    public float ProductionTime;
    public bool IsBusy { get; protected set; }

    public abstract bool CanProduce(UnitType type);
    public abstract void ProduceUnit(UnitType type);

    public void SetBusy(bool busy)
    {
        IsBusy = busy;
    }
}