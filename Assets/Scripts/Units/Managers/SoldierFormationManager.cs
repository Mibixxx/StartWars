using System.Collections.Generic;
using UnityEngine;

public class SoldierFormationManager
{
    private readonly Transform rallyPoint;
    private readonly List<MilitaryUnit> soldiers;

    private const int soldiersPerRow = 3;
    private const float spacing = 4f;

    public SoldierFormationManager(Transform rallyPoint, List<MilitaryUnit> soldiers)
    {
        this.rallyPoint = rallyPoint;
        this.soldiers = soldiers;
    }

    public Vector3 GetNextFormationPosition()
    {
        int index = soldiers.Count;
        int row = index / soldiersPerRow;
        int col = index % soldiersPerRow;

        float offsetX = (col - (soldiersPerRow - 1) / 2f) * spacing;
        float offsetZ = -row * spacing;

        Vector3 right = rallyPoint.right;
        Vector3 forward = rallyPoint.forward;

        Vector3 offset = right * offsetX + forward * offsetZ;
        return rallyPoint.position + offset;
    }

    public void RecalculateFormation()
    {
        Vector3 right = rallyPoint.right;
        Vector3 forward = rallyPoint.forward;

        for (int i = 0; i < soldiers.Count; i++)
        {
            int row = i / soldiersPerRow;
            int col = i % soldiersPerRow;

            float offsetX = (col - (soldiersPerRow - 1) / 2f) * spacing;
            float offsetZ = -row * spacing;

            Vector3 offset = right * offsetX + forward * offsetZ;
            Vector3 targetPosition = rallyPoint.position + offset;

            soldiers[i].MoveToPosition(targetPosition);
        }
    }
}
