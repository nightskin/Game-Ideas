using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorGen : MonoBehaviour
{
    [SerializeField] DungeonGen level;

    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateFloors();
        UpdateMesh();

        level.navigationBaker.surface.BuildNavMesh();
        level.navigationBaker.PlaceEnemies();
    }

    void OnDrawGizmos()
    {
        if(Application.isPlaying)
        {
            for (int x = 0; x < level.tilesX; x++)
            {
                for (int z = 0; z < level.tilesZ; z++)
                {
                    if (x < level.tilesX - 1 && z < level.tilesZ - 1)
                    {
                        Vector3[] corners =
                        {
                            level.map[x,z].position,
                            level.map[x, z + 1].position,
                            level.map[x + 1, z].position,
                            level.map[x + 1, z + 1].position,
                        };
                        Vector3[] midPoints =
                        {
                            level.map[x,z].position + new Vector3(0, 0, level.tileSize / 2),
                            level.map[x,z].position + new Vector3(level.tileSize, 0, level.tileSize / 2),
                            level.map[x,z].position + new Vector3(level.tileSize / 2, 0, level.tileSize),
                            level.map[x,z].position + new Vector3(level.tileSize / 2, 0, 0),
                        };

                        DungeonTile square = new DungeonTile(level.map[x, z].position, level.tileSize, corners, midPoints);
                        string state = level.GetState(level.map[x, z].on, level.map[x, z + 1].on, level.map[x + 1, z].on, level.map[x + 1, z + 1].on);
                        
                        
                    }
                }
            }
        }
    }

    void CreateQuad(Vector3 bottomLeft, Vector3 topLeft, Vector3 bottomRight, Vector3 topRight)
    {
        verts.Add(bottomLeft);
        verts.Add(topLeft);
        verts.Add(bottomRight);
        verts.Add(topRight);

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

    void CreateTri(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        verts.Add(point1);
        verts.Add(point2);
        verts.Add(point3);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));


        tris.Add(0 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        buffer += 3;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    
    void CreateFloors()
    {
        for (int x = 0; x < level.tilesX; x++)
        {
            for (int z = 0; z < level.tilesZ; z++)
            {
                if (x < level.tilesX - 1 && z < level.tilesZ - 1)
                {
                    Vector3[] corners =    
                    {
                        level.map[x,z].position,
                        level.map[x,z+1].position,
                        level.map[x+1,z].position,
                        level.map[x+1,z+1].position,
                    };
                    Vector3[] midPoints =
                    {
                        level.map[x,z].position + new Vector3(0, 0, level.tileSize/2),
                        level.map[x,z].position + new Vector3(level.tileSize, 0, level.tileSize/2),
                        level.map[x,z].position + new Vector3(level.tileSize/2, 0, level.tileSize),
                        level.map[x,z].position + new Vector3(level.tileSize/2, 0, 0),
                    };
                    
                    DungeonTile tile = new DungeonTile(level.map[x, z].position, level.tileSize, corners, midPoints);
                    
                    string state = level.GetState(level.map[x, z].on, level.map[x, z + 1].on, level.map[x + 1, z].on, level.map[x + 1, z + 1].on);
                    
                    if (state == "1000")
                    {
                        CreateTri(tile.bottomLeft, tile.centerLeft, tile.centerBottom);
                    }
                    if (state == "0100")
                    {
                        CreateTri(tile.topLeft, tile.centerTop, tile.centerLeft);
                    }
                    if (state == "0010")
                    {
                        CreateTri(tile.bottomRight, tile.centerBottom, tile.centerRight);
                    }
                    if (state == "0001")
                    {
                        CreateTri(tile.topRight, tile.centerRight, tile.centerTop);
                    }

                    if (state == "1100")
                    {
                        CreateQuad(tile.bottomLeft, tile.topLeft, tile.centerBottom, tile.centerTop);
                    }
                    if (state == "0011")
                    {
                        CreateQuad(tile.centerBottom, tile.centerTop, tile.bottomRight, tile.topRight);
                    }
                    if (state == "1001")
                    {
                        CreateTri(tile.bottomLeft, tile.centerLeft, tile.centerBottom);
                        CreateTri(tile.topRight, tile.centerRight, tile.centerTop);
                    }
                    if (state == "0110")
                    {
                        CreateTri(tile.topLeft, tile.centerTop, tile.centerLeft);
                        CreateTri(tile.bottomRight, tile.centerBottom, tile.centerRight);
                    }
                    if (state == "0101")
                    {
                        CreateQuad(tile.centerLeft, tile.topLeft, tile.centerRight, tile.topRight);
                    }
                    if (state == "1010")
                    {
                        CreateQuad(tile.bottomLeft, tile.centerLeft, tile.bottomRight, tile.centerRight);
                    }


                    if (state == "0111")
                    {
                        CreateTri(tile.topLeft, tile.centerTop, tile.centerLeft);
                        CreateTri(tile.bottomRight, tile.centerBottom, tile.centerRight);
                        CreateTri(tile.topRight, tile.centerRight, tile.centerTop);

                        CreateTri(tile.centerTop, tile.centerRight, tile.centerLeft);
                        CreateTri(tile.centerBottom, tile.centerLeft, tile.centerRight);

                    }
                    if (state == "1011")
                    {
                        CreateTri(tile.bottomLeft, tile.centerLeft, tile.centerBottom);
                        CreateTri(tile.bottomRight, tile.centerBottom, tile.centerRight);
                        CreateTri(tile.topRight, tile.centerRight, tile.centerTop);

                        CreateTri(tile.centerTop, tile.centerRight, tile.centerLeft);
                        CreateTri(tile.centerBottom, tile.centerLeft, tile.centerRight);
                    }
                    if (state == "1101")
                    {
                        CreateTri(tile.bottomLeft, tile.centerLeft, tile.centerBottom);
                        CreateTri(tile.topLeft, tile.centerTop, tile.centerLeft);
                        CreateTri(tile.topRight, tile.centerRight, tile.centerTop);

                        CreateTri(tile.centerTop, tile.centerRight, tile.centerLeft);
                        CreateTri(tile.centerBottom, tile.centerLeft, tile.centerRight);
                    }
                    if (state == "1110")
                    {
                        CreateTri(tile.bottomLeft, tile.centerLeft, tile.centerBottom);
                        CreateTri(tile.topLeft, tile.centerTop, tile.centerLeft);
                        CreateTri(tile.bottomRight, tile.centerBottom, tile.centerRight);

                        CreateTri(tile.centerTop, tile.centerRight, tile.centerLeft);
                        CreateTri(tile.centerBottom, tile.centerLeft, tile.centerRight);
                    }

                    if (state == "1111") CreateQuad(tile.bottomLeft, tile.topLeft, tile.bottomRight, tile.topRight);
                }
            }
        }
    }

}
