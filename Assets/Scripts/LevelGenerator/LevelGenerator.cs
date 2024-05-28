using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MapType
{
    TWO_DIMENSIONAL,
    THREE_DIMENSIONAL
}

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] MapType mapType;

    [SerializeField] GameObject wallTile;
    [SerializeField] GameObject floorTile;
    [SerializeField] GameObject climbTile;

    [SerializeField] string seed;
    [SerializeField] int steps = 100;
    [SerializeField] float stepSize = 2;
    

    Vector3 walkerPosition = new Vector3(0, 0, 0);
    List<Vector3> positions = new List<Vector3>();


    void Start()
    {
        Random.InitState(seed.GetHashCode());

        if(mapType == MapType.TWO_DIMENSIONAL ) 
        {
            Make2DMap();
            CreateWalls();
        }
        else if(mapType == MapType.THREE_DIMENSIONAL)
        {
            Make3DMap();
            CreateWalls();
            AddLadders();
        }
    }

    void Make2DMap()
    {
        positions.Add(walkerPosition);
        for (int step = 0; step < steps; step++)
        {
            int xz = Random.Range(0, 4);

            if (xz == 0)
            {
                walkerPosition += Vector3.right * stepSize;
            }
            else if (xz == 1)
            {
                walkerPosition += Vector3.left * stepSize;
            }
            else if (xz == 2)
            {
                walkerPosition += Vector3.forward * stepSize;
            }
            else if (xz == 3)
            {
                walkerPosition += Vector3.back * stepSize;
            }
            
            positions.Add(walkerPosition);

        }
        positions = positions.Distinct().ToList();
    }

    void Make3DMap()
    {
        positions.Add(walkerPosition);
        for (int step = 0; step < steps; step++)
        {
            int xyz = Random.Range(0, 6);

            if (xyz == 0)
            {
                walkerPosition += Vector3.right * stepSize;
            }
            else if (xyz == 1)
            {
                walkerPosition += Vector3.left * stepSize;
            }
            else if (xyz == 2)
            {
                walkerPosition += Vector3.forward * stepSize;
            }
            else if (xyz == 3)
            {
                walkerPosition += Vector3.back * stepSize;
            }
            else if (xyz == 5)
            {
                walkerPosition += Vector3.up * stepSize;
            }
            else if (xyz == 6)
            {
                walkerPosition += Vector3.down * stepSize;
            }

            positions.Add(walkerPosition);

        }
        positions = positions.Distinct().ToList();
    }

    void CreateWalls()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            if (!positions.Contains(positions[i] + Vector3.forward * stepSize))
            {
                Vector3 pos = positions[i] + new Vector3(0, 0, stepSize / 2);
                Instantiate(wallTile, pos, Quaternion.identity, transform);
                if (!positions.Contains(positions[i] + new Vector3(0,-1, 1) * stepSize))
                {

                }
            }
            if (!positions.Contains(positions[i] + Vector3.back * stepSize))
            {
                Vector3 pos = positions[i] + new Vector3(0, 0, -stepSize / 2);
                Instantiate(wallTile, pos, Quaternion.identity, transform);
                if (!positions.Contains(positions[i] + new Vector3(0, -1, -1) * stepSize))
                {

                }
            }

            if (!positions.Contains(positions[i] + Vector3.right * stepSize))
            {
                Vector3 pos = positions[i] + new Vector3(stepSize / 2, 0, 0);
                Instantiate(wallTile, pos, Quaternion.Euler(0, 90, 0), transform);
                if (!positions.Contains(positions[i] + new Vector3(1, -1, 0) * stepSize))
                {

                }
            }
            if (!positions.Contains(positions[i] + Vector3.left * stepSize))
            {
                Vector3 pos = positions[i] + new Vector3(-stepSize / 2, 0, 0);
                Instantiate(wallTile, pos, Quaternion.Euler(0, 90, 0), transform);
                if (!positions.Contains(positions[i] + new Vector3(-1, -1, 0) * stepSize))
                {

                }
            }

            if (!positions.Contains(positions[i] + Vector3.up * stepSize))
            {
                Instantiate(floorTile, positions[i] + (Vector3.up * stepSize), Quaternion.identity, transform);
            }

            if (!positions.Contains(positions[i] + Vector3.down * stepSize))
            {
                Instantiate(floorTile, positions[i], Quaternion.identity, transform);
            }

        }
    }

    void AddLadders()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            if (positions.Contains(positions[i] + Vector3.up * stepSize))
            {
                Instantiate(climbTile, positions[i], Quaternion.identity, transform);
            }
        }
    }

}
