using System.Collections.Generic;
using UnityEngine;

public static class RallyPointManager
{
    private static HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    private static float positionTolerance = 1f;

    public static Vector3 GetFreePosition(Vector3 center, float spacing, int maxPerRow = 5)
    {
        int index = 0;

        while (true) // loop infinito finché non trova una posizione libera
        {
            int row = index / maxPerRow;
            int col = index % maxPerRow;

            float xOffset = (col - (maxPerRow - 1) / 2f) * spacing;
            float zOffset = row * spacing;

            Vector3 candidate = center + new Vector3(xOffset, 0, zOffset);

            if (!IsOccupied(candidate))
            {
                occupiedPositions.Add(candidate);
                return candidate;
            }

            index++;
        }
    }

    public static void ReleasePosition(Vector3 pos)
    {
        occupiedPositions.RemoveWhere(p => Vector3.Distance(p, pos) <= positionTolerance);
    }

    public static bool IsOccupied(Vector3 position)
    {
        foreach (Vector3 occupied in occupiedPositions)
        {
            if (Vector3.Distance(occupied, position) <= positionTolerance)
                return true;
        }
        return false;
    }
}
