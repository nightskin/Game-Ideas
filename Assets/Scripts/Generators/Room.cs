using System.Collections.Generic;
using UnityEngine;

class Room
{
    public Vector3Int indexPosition;
    public Vector3Int indexSize;
    public List<Vector3Int> exits;

    public Room(Vector3Int indexPosition, Vector3Int indexSize)
    {
        this.indexPosition = indexPosition;
        this.indexSize = indexSize;
        exits = new List<Vector3Int>();
        exits.Add(indexPosition + new Vector3Int(indexSize.x - 1, 0, 0));
        exits.Add(indexPosition + new Vector3Int(-indexSize.x + 1, 0, 0));
        exits.Add(indexPosition + new Vector3Int(0, 0, indexSize.z - 1));
        exits.Add(indexPosition + new Vector3Int(0, 0, -indexSize.z + 1));
    }

    public Room(Vector3Int indexPosition)
    {
        this.indexPosition = indexPosition;
        indexSize = Vector3Int.zero;
        exits = new List<Vector3Int>();
    }

    public Vector3Int GetNearestExit(Vector3Int toIndex)
    {
        Vector3Int nearest = exits[0];
        for (int i = 1; i < exits.Count; i++)
        {
            if (Vector3Int.Distance(toIndex, exits[i]) < Vector3Int.Distance(toIndex, nearest))
            {
                nearest = exits[i];
            }
        }
        return nearest;
    }

    public void UpdateExits()
    {
        exits[0] = indexPosition + new Vector3Int(indexSize.x - 1, 0, 0);
        exits[1] = indexPosition + new Vector3Int(-indexSize.x + 1, 0, 0);
        exits[2] = indexPosition + new Vector3Int(0, 0, indexSize.z - 1);
        exits[3] = indexPosition + new Vector3Int(0, 0, -indexSize.z + 1);
    }

}