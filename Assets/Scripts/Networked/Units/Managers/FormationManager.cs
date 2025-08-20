using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class FormationManager
{
    private readonly List<MilitaryUnit_Networked> units;
    private const int unitsPerRow = 3;
    private const float spacing = 4f;

    public FormationManager(List<MilitaryUnit_Networked> units)
    {
        this.units = units;
    }

    public void MoveUnitsToFormation(Vector3 centerPoint, Vector3 forward)
    {
        if (units == null) return;

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        for (int i = 0; i < units.Count; i++)
        {
            int row = i / unitsPerRow;
            int col = i % unitsPerRow;

            float offsetX = (col - (unitsPerRow - 1) / 2f) * spacing;
            float offsetZ = -row * spacing;

            Vector3 offset = right * offsetX + forward * offsetZ;
            Vector3 targetPosition = centerPoint + offset;

            if (units[i].Object.HasStateAuthority)
                units[i].MoveToPosition(targetPosition);
        }
    }

    public Vector3 GetNextFormationPosition(Vector3 centerPoint, Vector3 forward)
    {
        int index = units.Count;
        int row = index / unitsPerRow;
        int col = index % unitsPerRow;

        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        float offsetX = (col - (unitsPerRow - 1) / 2f) * spacing;
        float offsetZ = -row * spacing;

        return centerPoint + right * offsetX + forward * offsetZ;
    }
}
