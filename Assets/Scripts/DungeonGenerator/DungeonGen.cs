using System;
using UnityEngine;

public class DungeonTile
{
    public Vector3 center;

    public Vector3 bottomLeft;
    public Vector3 topLeft;
    public Vector3 bottomRight;
    public Vector3 topRight;

    public Vector3 centerLeft;
    public Vector3 centerRight;
    public Vector3 centerTop;
    public Vector3 centerBottom;

    public DungeonTile(Vector3 pos, float size ,Vector3[] corners, Vector3[] midPoints)
    {
        bottomLeft = corners[0];
        topLeft = corners[1];
        bottomRight = corners[2];
        topRight = corners[3];

        centerLeft = midPoints[0];
        centerRight = midPoints[1];
        centerTop = midPoints[2];
        centerBottom = midPoints[3];
        center = centerLeft + new Vector3(size / 2, 0, 0);
    }

    public void Shift(Vector3 direction)
    {
        bottomLeft += direction;
        topLeft += direction;
        bottomRight += direction;
        topRight += direction;

        centerLeft += direction;
        centerRight += direction;
        centerTop += direction;
        centerBottom += direction;
        center += direction;

    }

    public static Vector3 Up(Vector3 pos, float magnitude)
    {
        return pos + (Vector3.up * magnitude);
    }

}

public class DungeonPoint
{
    public Vector3 position;
    public Vector3 direction;
    public int on;
    public float height;
    public DungeonPoint()
    {
        position = Vector3.zero;
        direction = Vector3.zero;
        on = 0;
        height = 0;
    }

}

public class DungeonGen : MonoBehaviour
{
    public NavigationBaker navigationBaker;
    public DungeonPoint[,] map = null;

    public int tilesX = 20;
    public int tilesZ = 20;

    public float tileSize = 10;
    public string seed = "";
    Vector3Int currentPos = Vector3Int.zero;


    void Awake()
    {
        if(!navigationBaker) navigationBaker = GetComponent<NavigationBaker>();
        if (seed == "") seed = DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());
        GenerateMap();
    }

    void GenerateMap()
    {
        map = new DungeonPoint[tilesX, tilesZ];

        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                float rampHeight = UnityEngine.Random.Range(0.0f, 1.0f);
                map[x, z] = new DungeonPoint();
                map[x, z].height = rampHeight;
                map[x, z].position = new Vector3(x * tileSize, 0, z * tileSize);
            }
        }

        //Random Walker
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                int direction = UnityEngine.Random.Range(1, 5);
                map[currentPos.x, currentPos.z].on = 1;
                if (direction == 1)
                {
                    if (currentPos.x < tilesX - 1)
                    {
                        currentPos.x++;
                        map[currentPos.x, currentPos.z].direction = Vector3.right;
                    }
                }
                else if (direction == 2)
                {
                    if (currentPos.x > 0)
                    {
                        currentPos.x--;
                        map[currentPos.x, currentPos.z].direction = Vector3.left;
                    }
                }
                else if (direction == 3)
                {
                    if (currentPos.z < tilesZ - 1)
                    {
                        currentPos.z++;
                        map[currentPos.x, currentPos.z].direction = Vector3.forward;
                    }
                }
                else if (direction == 4)
                {
                    if (currentPos.z > 0)
                    {
                        currentPos.z--;
                        map[currentPos.x, currentPos.z].direction = Vector3.back;
                    }

                }
            }
        }
    }

    public string GetState(int a, int b, int c, int d)
    {
        return a.ToString() + b.ToString() + c.ToString() + d.ToString();
    }

}