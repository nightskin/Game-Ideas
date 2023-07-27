using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Space
{
    public float x;
    public float z;
    public float y;
    public bool on;
    public Space(float xPos, float zPos)
    {
        x = xPos;
        z = zPos;
        y = 0;
        on = false;
    }
    public Space (float xpos, float yPos, float zPos)
    {
        x = xpos;
        y = yPos;
        z = zPos;
        on = false;
    }
}

public class LevelGen : MonoBehaviour
{
    public bool threeDimensions = false;
    public Space[,] map = null;
    public Space[,,] map3d = null;
    public int tilesX = 25;
    public int tilesZ = 25;
    public int tilesY = 25;
    public float stepSize = 10;
    Vector3Int currentPos;


    void Awake()
    {
        currentPos = Vector3Int.zero;

        if (!threeDimensions)
        {
            map = new Space[tilesX, tilesZ];
            for (int x = 0; x < tilesX; x++)
            {
                for (int z = 0; z < tilesZ; z++)
                {
                    map[x, z] = new Space(x, z);
                }
            }
            Gen1();
        }
        else
        {
            map3d = new Space[tilesX, tilesY, tilesZ];
            for (int x = 0; x < tilesX; x++)
            {
                for (int y = 0; y < tilesY; y++)
                {
                    for (int z = 0; z < tilesZ; z++)
                    {
                        map3d[x, y, z] = new Space(x, y, z);
                    }
                }
            }
            Gen2();
        }

    }



    //2D Random Walker
    public void Gen1()
    {
        //Random Walker
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                int dir = Random.Range(0, 5);
                if (map[currentPos.x, currentPos.z].on == false)
                {
                    map[currentPos.x, currentPos.z].on = true;
                }
                if (dir == 1)
                {
                    if(currentPos.x < tilesX - 1)
                    {
                        currentPos.x++;
                    }
                
                }
                else if (dir == 2)
                {
                    if(currentPos.x > 0)
                    {
                        currentPos.x--;
                    }
                
                }
                else if (dir == 3)
                {
                    if(currentPos.z < tilesZ - 1)
                    {
                        currentPos.z++;
                    }
                
                }
                else if (dir == 4)
                {
                    if (currentPos.z > 0)
                    {
                        currentPos.z--;
                    }
                }
            }
        }
    }

    //3D Random Walker
    public void Gen2()
    {
        //Random Walker
        for (int x = 0; x < tilesX; x++)
        {
            for(int y = 0; y < tilesY; y++)
            {
                for (int z = 0; z < tilesZ; z++)
                {
                    int dir = Random.Range(0, 7);
                    if (map3d[currentPos.x, currentPos.y, currentPos.z].on == false)
                    {
                        map3d[currentPos.x, currentPos.y, currentPos.z].on = true;
                    }
                    if (dir == 1)
                    {
                        if (currentPos.x < tilesX - 1)
                        {
                            currentPos.x++;
                        }

                    }
                    else if (dir == 2)
                    {
                        if (currentPos.x > 0)
                        {
                            currentPos.x--;
                        }

                    }
                    else if (dir == 3)
                    {
                        if (currentPos.z < tilesZ - 1)
                        {
                            currentPos.z++;
                        }

                    }
                    else if (dir == 4)
                    {
                        if (currentPos.z > 0)
                        {
                            currentPos.z--;
                        }
                    }
                    else if (dir == 5)
                    {
                        if (currentPos.y > 0)
                        {
                            currentPos.y--;
                        }
                    }
                    else if (dir == 6)
                    {
                        if (currentPos.y < tilesY - 1)
                        {
                            currentPos.y++;
                        }
                    }
                }
            }

        }
    }



}
