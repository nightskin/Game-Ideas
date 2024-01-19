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

    public Square(Vector3 pos, float size ,Point[] corners, Vector3[] midPoints)
    {
        center = pos;
        bottomLeft = corners[0].position;
        topLeft = corners[1].position;
        bottomRight = corners[2].position;
        topRight = corners[3].position;

        centerLeft = midPoints[0];
        centerRight = midPoints[1];
        centerTop = midPoints[2];
        centerBottom = midPoints[3];
    }

    public Square(Vector3 pos, float size)
    {
        center = pos;

        bottomLeft = new Vector3(pos.x - size / 2, pos.y, pos.z - size / 2);
        bottomRight = new Vector3(bottomLeft.x + size, bottomLeft.y, bottomLeft.z);
        topLeft = new Vector3(bottomLeft.x, bottomLeft.y, bottomLeft.z + size);
        topRight = new Vector3(topLeft.x + size, topLeft.y, topLeft.z);

        centerLeft = new Vector3(pos.x - size / 2, pos.y, pos.z);
        centerBottom = new Vector3(pos.x, pos.y, pos.z - size/2);
        centerTop = new Vector3(pos.x, pos.y, pos.z + size/2);
        centerRight = new Vector3(pos.x + size / 2, pos.y, pos.z);
    }
    
    public Vector3 PlusUp(Vector3 pos, float magnitude)
    {
        return pos + (Vector3.up * magnitude);
    }

}

public class Point
{
    public Vector3 position;
    public int on;
    public Point()
    {
        position = new Vector3();
        on = 0;
    }

}

public class DungeonGen : MonoBehaviour
{
    public Point[,] map = null;
    public int tilesX = 20;
    public int tilesZ = 20;

    public float tileSize = 10;
    public float maxRampHeight = 5;
    public string seed = "";
    public System.Random random;
    Vector3Int currentPos = Vector3Int.zero;


    void Awake()
    {
        if (seed == "") 
        {
            string date = DateTime.Now.ToString();
            seed = date;
        }
        
        random = new System.Random(seed.GetHashCode());

        GenerateMap();

    }

    void GenerateMap()
    {
        map = new Point[tilesX, tilesZ];
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                map[x, z] = new Point();
                map[x, z].position = new Vector3(x * tileSize, 0, z * tileSize);
            }
        }


        //Random Walker
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                int dir;
                dir = random.Next(1, 5);
                map[currentPos.x, currentPos.z].on = 1;
                if (dir == 1)
                {
                    if (currentPos.x < tilesX - 1)
                    {
                        currentPos.x++;
                    }
                }
                if (dir == 2)
                {
                    if (currentPos.x > 0)
                    {
                        currentPos.x--;
                    }
                }
                if (dir == 3)
                {
                    if (currentPos.z < tilesZ - 1)
                    {
                        currentPos.z++;
                    }
                }
                if (dir == 4)
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
