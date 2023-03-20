using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Space
{
    public float x;
    public float z;
    public float y;
    public bool visited;
    public Space(float xPos, float zPos)
    {
        x = xPos;
        z = zPos;
        y = 0;
        visited = false;
    }

    public Space (float xpos, float yPos, float zPos)
    {
        x = xpos;
        y = yPos;
        z = zPos;
        visited = false;
    }
}

public class LevelGen : MonoBehaviour
{
    
    Space[,] map2d;
    Space[,,] map3d;

    public Transform player;
    public GameObject spiderPrefab;
    public int tilesX = 25;
    public int tilesY = 25;
    public int tilesZ = 25;
    public float stepSize = 10;
    public bool level3D = false;

    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<Vector3> normals = new List<Vector3>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        if(level3D)
        {
            CreateLevel3D();
        }
        else
        {
            CreateLevel2D();
            AddMonsters();
        }

        UpdateMesh();

    }

    void CreateQuad(Vector3 bottomLeft, Vector3 topLeft, Vector3 bottomRight, Vector3 topRight, Vector3 offset)
    {
        verts.Add(bottomLeft + offset);
        verts.Add(topLeft + offset);
        verts.Add(bottomRight + offset);
        verts.Add(topRight + offset);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));

        tris.Add(0 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        tris.Add(1 + buffer);
        tris.Add(3 + buffer);
        tris.Add(2 + buffer);

        buffer += 4;
    }

    void CreateBox(Vector3 offset, Vector3 size)
    {
        //top
        CreateQuad(new Vector3(-1 * size.x, 1 * size.y, -1 * size.z), new Vector3(-1 * size.x, 1 * size.y, 1 * size.z), new Vector3(1 * size.x, 1 * size.y, -1 * size.z), new Vector3(1 * size.x, 1 * size.y, 1 * size.z), offset);
        //Bottom
        CreateQuad(new Vector3(-1 * size.x, -1 * size.y, 1 * size.z), new Vector3(-1 * size.x, -1 * size.y, -1 * size.z), new Vector3(1 * size.x, -1 * size.y, 1 * size.z), new Vector3(1 * size.x, -1 * size.y, -1 * size.z), offset);
        //Left
        CreateQuad(new Vector3(-1 * size.x, 1 * size.y, -1 * size.z), new Vector3(-1 * size.x, -1 * size.y, -1 * size.z), new Vector3(-1 * size.x, 1 * size.y, 1 * size.z), new Vector3(-1 * size.x, -1 * size.y, 1 * size.z), offset);
        //Right
        CreateQuad(new Vector3(1 * size.x, -1 * size.y, -1 * size.z), new Vector3(1 * size.x, 1 * size.y, -1 * size.z), new Vector3(1 * size.x, -1 * size.y, 1 * size.z), new Vector3(1 * size.x, 1 * size.y, 1 * size.z), offset);
        //Front
        CreateQuad(new Vector3(-1 * size.x, 1 * size.y, 1 * size.z), new Vector3(-1 * size.x, -1 * size.y, 1 * size.z), new Vector3(1 * size.x, 1 * size.y, 1 * size.z), new Vector3(1 * size.x, -1 * size.y, 1 * size.z), offset);
        //Back
        CreateQuad(new Vector3(-1 * size.x, -1 * size.y, -1 * size.z), new Vector3(-1 * size.x, 1 * size.y, -1 * size.z), new Vector3(1 * size.x, -1 * size.y, -1 * size.z), new Vector3(1 * size.x, 1 * size.y, -1 * size.z), offset);
    }

    void CreateWalls(Vector3 offset, Vector3 size, bool front = false, bool back = false, bool left = false, bool right = false, bool up = false, bool down = false)
    {
        //Left
        if (left)
            CreateQuad(new Vector3(-1 * size.x, -1 * size.y, -1 * size.z), new Vector3(-1 * size.x, 1 * size.y, -1 * size.z), new Vector3(-1 * size.x, -1 * size.y, 1 * size.z), new Vector3(-1 * size.x, 1 * size.y, 1 * size.z), offset);

        //Right
        if (right)
            CreateQuad(new Vector3(1 * size.x, 1 * size.y, -1 * size.z), new Vector3(1 * size.x, -1 * size.y, -1 * size.z), new Vector3(1 * size.x, 1 * size.y, 1 * size.z), new Vector3(1 * size.x, -1 * size.y, 1 * size.z), offset);

        //Back
        if (back)
            CreateQuad(new Vector3(-1 * size.x, 1 * size.y, -1 * size.z), new Vector3(-1 * size.x, -1 * size.y, -1 * size.z), new Vector3(1 * size.x, 1 * size.y, -1 * size.z), new Vector3(1 * size.x, -1 * size.y, -1 * size.z), offset);

        //Front
        if (front)
            CreateQuad(new Vector3(-1 * size.x, -1 * size.y, 1 * size.z), new Vector3(-1 * size.x, 1 * size.y, 1 * size.z), new Vector3(1 * size.x, -1 * size.y, 1 * size.z), new Vector3(1 * size.x, 1 * size.y, 1 * size.z), offset);

        //Up
        if(up)
            CreateQuad(new Vector3(-1 * size.x, 1 * size.y, 1 * size.z), new Vector3(-1 * size.x, 1 * size.y, -1 * size.z), new Vector3(1 * size.x, 1 * size.y, 1 * size.z), new Vector3(1 * size.x, 1 * size.y, -1 * size.z), offset);

        if(down)
            CreateQuad(new Vector3(-1 * size.x, -1 * size.y, -1 * size.z), new Vector3(-1 * size.x, -1 * size.y, 1 * size.z), new Vector3(1 * size.x, -1 * size.y, -1 * size.z), new Vector3(1 * size.x, -1 * size.y, 1 * size.z), offset);

    }
    
    void CreateFloor(Vector3 offset, Vector3 size, bool ceiling = false)
    {
        //Ceiling
        if(ceiling)
            CreateQuad(new Vector3(-1 * size.x, 1 * size.y, 1 * size.z), new Vector3(-1 * size.x, 1 * size.y, -1 * size.z), new Vector3(1 * size.x, 1 * size.y, 1 * size.z), new Vector3(1 * size.x, 1 * size.y, -1 * size.z), offset);
        
        //Floor
        CreateQuad(new Vector3(-1 * size.x, -1 * size.y, -1 * size.z), new Vector3(-1 * size.x, -1 * size.y, 1 * size.z), new Vector3(1 * size.x, -1 * size.y, -1 * size.z), new Vector3(1 * size.x, -1 * size.y, 1 * size.z), offset);
    }

    public void CreateLevel2D()
    {
        map2d = new Space[tilesX, tilesZ];
        Vector3Int currentPos = Vector3Int.zero;

        //Create Map
        for(int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                map2d[x, z] = new Space(x,z);
            }
        }

        //Random Walker
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                int dir = Random.Range(1, 5);
                if (map2d[currentPos.x, currentPos.z].visited == false)
                {
                    CreateFloor((Vector3)currentPos * stepSize, Vector3.one * stepSize / 2, true);
                    map2d[currentPos.x, currentPos.z].visited = true;
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

        //Create Walls
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                if (map2d[x,z].visited)
                {
                    if (x == 0)
                    {
                        CreateWalls(new Vector3(x, 0, z) * stepSize, Vector3.one * stepSize / 2, false, false, true);
                    }
                    if (x > 0)
                    {
                        if (!map2d[x - 1, z].visited)
                        {
                            CreateWalls(new Vector3(x, 0, z) * stepSize, Vector3.one * stepSize / 2, false, false, true);
                        }
                    }

                    if (x < tilesX - 1)
                    {
                        if (!map2d[x + 1, z].visited)
                        {
                            CreateWalls(new Vector3(x, 0, z) * stepSize, Vector3.one * stepSize / 2, false, false, false, true);
                        }
                    }
                    if (x == tilesX - 1)
                    {
                        CreateWalls(new Vector3(x, 0, z) * stepSize, Vector3.one * stepSize / 2, false, false, false, true);
                    }

                    if (z == 0)
                    {
                        CreateWalls(new Vector3(x, 0, z) * stepSize, Vector3.one * stepSize / 2, false, true);
                    }
                    if (z > 0)
                    {
                        if (!map2d[x, z - 1].visited)
                        {
                            CreateWalls(new Vector3(x, 0, z) * stepSize, Vector3.one * stepSize / 2, false, true);
                        }
                    }

                    if (z < tilesZ - 1)
                    {
                        if (!map2d[x, z + 1].visited)
                        {
                            CreateWalls(new Vector3(x, 0, z) * stepSize, Vector3.one * stepSize / 2, true);
                        }
                    }
                    if (z == tilesZ - 1)
                    {
                        CreateWalls(new Vector3(x, 0, z) * stepSize, Vector3.one * stepSize / 2, true);
                    }
                }
            }
        }
    }

    void AddMonsters()
    {
        if(level3D)
        {

        }
        else
        {
            //Create Map
            for (int x = 0; x < tilesX; x++)
            {
                for (int z = 0; z < tilesZ; z++)
                {
                    if (map2d[x, z].visited && x != 0 && z != 0)
                    {
                        float r = Random.Range(0.0f, 1.0f) * 100;
                        if(r > 90.0f)
                        {
                            GameObject enemy =  Instantiate(spiderPrefab, new Vector3(x, 0, z) * stepSize, Quaternion.identity, transform);
                            enemy.GetComponent<GroundEnemyAI>().target = player;
                        }
                    }
                }
            }
        }
    }

    public void CreateLevel3D()
    {
        map3d = new Space[tilesX, tilesY, tilesZ];
        Vector3Int currentPos = Vector3Int.zero;

        //CreateMap
        for(int x = 0; x < tilesX; x++)
        {
            for(int y = 0; y < tilesY; y++)
            {
                for(int z = 0; z < tilesZ; z++)
                {
                    map3d[x, y, z] = new Space(x, y, z);
                }
            }
        }

        //Random Walker
        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                for (int z = 0; z < tilesZ; z++)
                {
                    int dir = Random.Range(1, 7);
                    if (map3d[currentPos.x, currentPos.y, currentPos.z].visited == false)
                    {
                        map3d[currentPos.x, currentPos.y ,currentPos.z].visited = true;
                    }
                    if(dir == 1)
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
                    else if(dir == 5)
                    {
                        if(currentPos.y > 0)
                        {
                            currentPos.y--;
                        }
                    }
                    else if(dir == 6)
                    {
                        if(currentPos.y < tilesY - 1)
                        {
                            currentPos.y++;
                        }
                    }
                }
            }
        }

        //Create Walls
        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                for (int z = 0; z < tilesZ; z++)
                {
                    if(map3d[x,y,z].visited)
                    {
                        if (x == 0)
                        {
                            CreateWalls(new Vector3(x, y, z) * stepSize, Vector3.one * stepSize / 2, false, false, true);
                        }
                        if (x > 0)
                        {
                            if (!map3d[x - 1, y,z].visited)
                            {
                                CreateWalls(new Vector3(x, y, z) * stepSize, Vector3.one * stepSize / 2, false, false, true);
                            }
                        }
                        if (x < tilesX - 1)
                        {
                            if (!map3d[x + 1, y,z].visited)
                            {
                                CreateWalls(new Vector3(x, y, z) * stepSize, Vector3.one * stepSize / 2, false, false, false, true);
                            }
                        }
                        if (x == tilesX - 1)
                        {
                            CreateWalls(new Vector3(x, y, z) * stepSize, Vector3.one * stepSize / 2, false, false, false, true);
                        }

                        if (z == 0)
                        {
                            CreateWalls(new Vector3(x, y, z) * stepSize, Vector3.one * stepSize / 2, false, true);
                        }
                        if (z > 0)
                        {
                            if (!map3d[x, y,z - 1].visited)
                            {
                                CreateWalls(new Vector3(x, y, z) * stepSize, Vector3.one * stepSize / 2, false, true);
                            }
                        }
                        if (z < tilesZ - 1)
                        {
                            if (!map3d[x, y,z + 1].visited)
                            {
                                CreateWalls(new Vector3(x, y, z) * stepSize, Vector3.one * stepSize / 2, true);
                            }
                        }
                        if (z == tilesZ - 1)
                        {
                            CreateWalls(new Vector3(x, y, z) * stepSize, Vector3.one * stepSize / 2, true);
                        }

                        if(y > 0)
                        {
                            if(!map3d[x,y - 1,z].visited)
                            {
                                CreateWalls(new Vector3(x, y, z) * stepSize, Vector3.one * stepSize / 2, false, false, false, false, false, true);
                            }

                        }
                        if(y < tilesY - 1)
                        {
                            if (!map3d[x, y + 1, z].visited)
                            {
                                CreateWalls(new Vector3(x, y, z) * stepSize, Vector3.one * stepSize / 2, false, false, false, false, true);
                            }
                        }
                        if(y == 0)
                        {
                            CreateWalls(new Vector3(x, y, z) * stepSize, Vector3.one * stepSize / 2, false, false, false, false, false, true);
                        }
                        if(y == tilesY - 1)
                        {
                            CreateWalls(new Vector3(x, y, z) * stepSize, Vector3.one * stepSize / 2, false, false, false, false, true);
                        }
                    }
                }
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    

}
