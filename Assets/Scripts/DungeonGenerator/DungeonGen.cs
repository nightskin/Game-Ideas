using System;
using UnityEngine;

public class Square
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

    public Square(Vector3 pos, float size ,Vector3[] corners, Vector3[] midPoints)
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

    
    public static Vector3 Up(Vector3 pos, float magnitude)
    {
        return pos + (Vector3.up * magnitude);
    }

}

public class DungoenPoint
{
    public Vector3 position;
    public int on;
    public DungoenPoint()
    {
        position = Vector3.zero;
        on = 0;
    }

}

public class DungeonGen : MonoBehaviour
{
    public NavigationBaker navigationBaker;
    public DungoenPoint[,] map = null;

    public int tilesX = 20;
    public int tilesZ = 20;

    public float tileSize = 10;
    public float maxRampHeight = 5;
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
        map = new DungoenPoint[tilesX, tilesZ];

        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                map[x, z] = new DungoenPoint();
                map[x, z].position = new Vector3(x * tileSize, 0, z * tileSize);
            }
        }

        //Random Walker
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                int dirXZ = UnityEngine.Random.Range(0, 5);
                map[currentPos.x, currentPos.z].on = 1;
                if (dirXZ == 1)
                {
                    if (currentPos.x < tilesX - 1)
                    {
                        currentPos.x++;
                    }
                }
                else if (dirXZ == 2)
                {
                    if (currentPos.x > 0)
                    {
                        currentPos.x--;
                    }
                }
                else if (dirXZ == 3)
                {
                    if (currentPos.z < tilesZ - 1)
                    {
                        currentPos.z++;
                    }
                }
                else if (dirXZ == 4)
                {
                    if (currentPos.z > 0)
                    {
                        currentPos.z--;
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
