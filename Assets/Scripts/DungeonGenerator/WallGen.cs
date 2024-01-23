using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGen : MonoBehaviour
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

        MakeWalls();

        UpdateMesh();

    }

    void OnDrawGizmos()
    {
        if(level.map != null)
        {
            for (int x = 0; x < level.tilesX; x++)
            {
                for (int z = 0; z < level.tilesZ; z++)
                {
                    if (x < level.tilesX - 1 && z < level.tilesZ - 1)
                    {
                        Vector3[] corners =
                        {
                            level.map[x, z].position,
                            level.map[x, z + 1].position,
                            level.map[x + 1, z].position,
                            level.map[x + 1, z + 1].position,
                        };

                        Vector3[] midPoints =
                        {
                            level.map[x,z].position + new Vector3(0, 0, level.tileSize/2),
                            level.map[x,z].position + new Vector3(level.tileSize, 0, level.tileSize/2),
                            level.map[x,z].position + new Vector3(level.tileSize/2, 0, level.tileSize),
                            level.map[x,z].position + new Vector3(level.tileSize/2, 0, 0),
                        };

                        Square Square = new Square(level.map[x, z].position, level.tileSize, corners, midPoints);
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

    void MakeWalls()
    {
        for (int x = 0; x < level.tilesX; x++)
        {
            for (int z = 0; z < level.tilesZ; z++)
            {
                if (x < level.tilesX - 1 && z < level.tilesZ - 1)
                {
                    Vector3[] corners =
                    {
                        level.map[x, z].position,
                        level.map[x, z + 1].position,
                        level.map[x + 1, z].position,
                        level.map[x + 1, z + 1].position,
                    };

                    Vector3[] midPoints =
                    {
                        level.map[x,z].position + new Vector3(0, 0, level.tileSize/2),
                        level.map[x,z].position + new Vector3(level.tileSize, 0, level.tileSize/2),
                        level.map[x,z].position + new Vector3(level.tileSize/2, 0, level.tileSize),
                        level.map[x,z].position + new Vector3(level.tileSize/2, 0, 0),
                    };

                    Square square = new Square(level.map[x, z].position, level.tileSize, corners, midPoints);
                    string state = level.GetState(level.map[x, z].on, level.map[x, z + 1].on, level.map[x + 1, z].on, level.map[x + 1, z + 1].on);

                    if (state == "1000")
                    {
                        CreateQuad(square.centerLeft, Square.Up(square.centerLeft, level.tileSize), square.centerBottom, Square.Up(square.centerBottom, level.tileSize));
                        if (x == 0) CreateQuad(Square.Up(square.centerLeft, level.tileSize), square.centerLeft, Square.Up(square.bottomLeft, level.tileSize), square.bottomLeft);
                        if (z == 0) CreateQuad(square.centerBottom, Square.Up(square.centerBottom, level.tileSize), square.bottomLeft, Square.Up(square.bottomLeft, level.tileSize));
                    }
                    else if (state == "0100")
                    {
                        CreateQuad(Square.Up(square.centerLeft, level.tileSize), square.centerLeft, Square.Up(square.centerTop, level.tileSize), square.centerTop);
                        if (x == 0) CreateQuad(Square.Up(square.centerLeft, level.tileSize), square.centerLeft, Square.Up(square.topLeft, level.tileSize), square.topLeft);
                        if (z == level.tilesZ - 2) CreateQuad(Square.Up(square.centerTop, level.tileSize), square.centerTop, Square.Up(square.topRight, level.tileSize), square.topRight);
                    }
                    else if (state == "0010")
                    {
                        CreateQuad(Square.Up(square.centerRight, level.tileSize), square.centerRight, Square.Up(square.centerBottom, level.tileSize), square.centerBottom);
                        if (x == level.tilesX - 2) CreateQuad(square.centerRight, Square.Up(square.centerRight, level.tileSize), square.bottomRight, Square.Up(square.bottomRight, level.tileSize));
                        if (z == 0) CreateQuad(square.bottomRight, Square.Up(square.bottomRight, level.tileSize), square.centerBottom, Square.Up(square.centerBottom, level.tileSize));
                    }
                    else if (state == "0001")
                    {
                        CreateQuad(square.centerRight, Square.Up(square.centerRight, level.tileSize), square.centerTop, Square.Up(square.centerTop, level.tileSize));
                        if (z == level.tilesZ - 2) CreateQuad(Square.Up(square.topRight, level.tileSize), square.topRight, Square.Up(square.centerTop, level.tileSize), square.centerTop);
                        if (x == level.tilesX - 2) CreateQuad(Square.Up(square.centerRight, level.tileSize), square.centerRight, Square.Up(square.topRight, level.tileSize), square.topRight);
                    }

                    else if (state == "1100")
                    {
                        CreateQuad(Square.Up(square.centerBottom, level.tileSize), square.centerBottom, Square.Up(square.centerTop, level.tileSize), square.centerTop);
                        if (x == 0) CreateQuad(square.bottomLeft, Square.Up(square.bottomLeft, level.tileSize), square.topLeft, Square.Up(square.topLeft, level.tileSize));
                        if (z == 0) CreateQuad(Square.Up(square.bottomLeft, level.tileSize), square.bottomLeft, Square.Up(square.centerBottom, level.tileSize), square.centerBottom);
                        if (z == level.tilesZ - 2) CreateQuad(square.topLeft, Square.Up(square.topLeft, level.tileSize), square.centerTop, Square.Up(square.centerTop, level.tileSize));
                    }
                    else if (state == "0011")
                    {
                        CreateQuad(square.centerBottom, Square.Up(square.centerBottom, level.tileSize), square.centerTop, Square.Up(square.centerTop, level.tileSize));
                        if (z == 0) CreateQuad(Square.Up(square.centerBottom, level.tileSize), square.centerBottom, Square.Up(square.bottomRight, level.tileSize), square.bottomRight);
                        if (x == level.tilesX - 2) CreateQuad(Square.Up(square.bottomRight, level.tileSize), square.bottomRight, Square.Up(square.topRight, level.tileSize), square.topRight);
                        if (z == level.tilesZ - 2) CreateQuad(square.centerTop, Square.Up(square.centerTop, level.tileSize), square.topRight, Square.Up(square.topRight, level.tileSize));
                    }
                    else if (state == "1001")
                    {
                        CreateQuad(square.centerLeft, Square.Up(square.centerLeft, level.tileSize), square.centerBottom, Square.Up(square.centerBottom, level.tileSize));
                        CreateQuad(square.centerRight, Square.Up(square.centerRight, level.tileSize), square.centerTop, Square.Up(square.centerTop, level.tileSize));
                        if (x == 0) CreateQuad(Square.Up(square.centerLeft, level.tileSize), square.centerLeft, Square.Up(square.bottomLeft, level.tileSize), square.bottomLeft);
                        if (z == level.tilesZ - 2) CreateQuad(Square.Up(square.topLeft, level.tileSize), square.topLeft, Square.Up(square.centerTop, level.tileSize), square.centerTop);

                    }
                    else if (state == "0110")
                    {
                        CreateQuad(Square.Up(square.centerLeft, level.tileSize), square.centerLeft, Square.Up(square.centerTop, level.tileSize), square.centerTop);
                        CreateQuad(Square.Up(square.centerRight, level.tileSize), square.centerRight, Square.Up(square.centerBottom, level.tileSize), square.centerBottom);
                        if (z == 0) CreateQuad(square.centerBottom, Square.Up(square.centerBottom, level.tileSize), square.bottomLeft, Square.Up(square.bottomLeft, level.tileSize));
                        if (x == level.tilesX - 2) CreateQuad(Square.Up(square.centerRight, level.tileSize), square.centerRight, Square.Up(square.bottomRight, level.tileSize), square.bottomRight);
                    }
                    else if (state == "0101")
                    {
                        CreateQuad(Square.Up(square.centerLeft, level.tileSize), square.centerLeft, Square.Up(square.centerRight, level.tileSize), square.centerRight);
                        if (z == level.tilesZ - 2) CreateQuad(square.topLeft, Square.Up(square.topLeft, level.tileSize), square.topRight, Square.Up(square.topRight, level.tileSize));
                        if (x == 0) CreateQuad(square.centerLeft, Square.Up(square.centerLeft, level.tileSize), square.topLeft, Square.Up(square.topLeft, level.tileSize));
                        if (x == level.tilesX - 2) CreateQuad(Square.Up(square.centerRight, level.tileSize), square.centerRight, Square.Up(square.topRight, level.tileSize), square.topRight);
                    }
                    else if (state == "1010")
                    {
                        CreateQuad(square.centerLeft, Square.Up(square.centerLeft, level.tileSize), square.centerRight, Square.Up(square.centerRight, level.tileSize));
                        if (x == 0) CreateQuad(Square.Up(square.centerLeft, level.tileSize), square.centerLeft, Square.Up(square.bottomLeft, level.tileSize), square.bottomLeft);
                        if (x == level.tilesX - 2) CreateQuad(square.centerRight, Square.Up(square.centerRight, level.tileSize), square.bottomRight, Square.Up(square.bottomRight, level.tileSize));
                        if (z == 0) CreateQuad(Square.Up(square.bottomLeft, level.tileSize), square.bottomLeft, Square.Up(square.bottomRight, level.tileSize), square.bottomRight);
                    }

                    else if (state == "0111")
                    {
                        CreateQuad(Square.Up(square.centerLeft, level.tileSize), square.centerLeft, Square.Up(square.centerBottom, level.tileSize), square.centerBottom);
                        if (z == 0) CreateQuad(Square.Up(square.centerBottom, level.tileSize), square.centerBottom, Square.Up(square.bottomRight, level.tileSize), square.bottomRight);
                        if (x == 0) CreateQuad(square.centerLeft, Square.Up(square.centerLeft, level.tileSize), square.topLeft, Square.Up(square.topLeft, level.tileSize));
                        if (x == level.tilesX - 2) CreateQuad(Square.Up(square.bottomRight, level.tileSize), square.bottomRight, Square.Up(square.topRight, level.tileSize), square.topRight);
                        if (z == level.tilesZ - 2) CreateQuad(square.topLeft, Square.Up(square.topLeft, level.tileSize), square.topRight, Square.Up(square.topRight, level.tileSize));
                    }
                    else if (state == "1011")
                    {
                        CreateQuad(square.centerLeft, Square.Up(square.centerLeft, level.tileSize), square.centerTop, Square.Up(square.centerTop, level.tileSize));
                        if (x == 0) CreateQuad(square.bottomLeft, Square.Up(square.bottomLeft, level.tileSize), square.centerLeft, Square.Up(square.centerLeft, level.tileSize));
                        if (z == level.tilesZ - 2) CreateQuad(square.centerTop, Square.Up(square.centerTop, level.tileSize), square.topRight, Square.Up(square.topRight, level.tileSize));
                        if (z == 0) CreateQuad(Square.Up(square.bottomLeft, level.tileSize), square.bottomLeft, Square.Up(square.bottomRight, level.tileSize), square.bottomRight);
                        if (x == level.tilesX - 2) CreateQuad(Square.Up(square.bottomRight, level.tileSize), square.bottomRight, Square.Up(square.topRight, level.tileSize), square.topRight);
                    }
                    else if (state == "1101")
                    {
                        CreateQuad(square.centerRight, Square.Up(square.centerRight, level.tileSize), square.centerBottom, Square.Up(square.centerBottom, level.tileSize));
                        if (x == level.tilesX - 2) CreateQuad(Square.Up(square.centerRight, level.tileSize), square.centerRight, Square.Up(square.topRight, level.tileSize), square.topRight);
                        if (z == 0) CreateQuad(Square.Up(square.bottomLeft, level.tileSize), square.bottomLeft, Square.Up(square.centerBottom, level.tileSize), square.centerBottom);
                        if (x == 0) CreateQuad(square.bottomLeft, Square.Up(square.bottomLeft, level.tileSize), square.topLeft, Square.Up(square.topLeft, level.tileSize));
                        if (z == level.tilesZ - 2) CreateQuad(square.topLeft, Square.Up(square.topLeft, level.tileSize), square.topRight, Square.Up(square.topRight, level.tileSize));
                    }
                    else if (state == "1110")
                    {
                        CreateQuad(Square.Up(square.centerRight, level.tileSize), square.centerRight, Square.Up(square.centerTop, level.tileSize), square.centerTop);
                        if (z == level.tilesZ - 2) CreateQuad(square.topLeft, Square.Up(square.topLeft, level.tileSize), square.centerTop, Square.Up(square.centerTop, level.tileSize));
                        if (x == level.tilesX - 2) CreateQuad(square.centerRight, Square.Up(square.centerRight, level.tileSize), square.bottomRight, Square.Up(square.bottomRight, level.tileSize));
                        if (x == 0) CreateQuad(square.bottomLeft, Square.Up(square.bottomLeft, level.tileSize), square.topLeft, Square.Up(square.topLeft, level.tileSize));
                        if (z == 0) CreateQuad(Square.Up(square.bottomLeft, level.tileSize), square.bottomLeft, Square.Up(square.bottomRight, level.tileSize), square.bottomRight);
                    }

                    else if (state == "1111")
                    {
                        if (x == 0) CreateQuad(square.bottomLeft, Square.Up(square.bottomLeft, level.tileSize), square.topLeft, Square.Up(square.topLeft, level.tileSize));
                        if (z == 0) CreateQuad(Square.Up(square.bottomLeft, level.tileSize), square.bottomLeft, Square.Up(square.bottomRight, level.tileSize), square.bottomRight);
                        if (x == level.tilesX - 2) CreateQuad(Square.Up(square.bottomRight, level.tileSize), square.bottomRight, Square.Up(square.topRight, level.tileSize), square.topRight);
                        if (z == level.tilesZ - 2) CreateQuad(square.topLeft, Square.Up(square.topLeft, level.tileSize), square.topRight, Square.Up(square.topRight, level.tileSize));
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
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
