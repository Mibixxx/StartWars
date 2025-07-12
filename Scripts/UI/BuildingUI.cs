using UnityEngine;

public class BuildingUI : MonoBehaviour
{
    public BuildingBase currentBuilding;

    public void ProduceCitizen() => currentBuilding.ProduceUnit(UnitType.Citizen);
    public void ProduceSoldier() => currentBuilding.ProduceUnit(UnitType.Soldier);
}