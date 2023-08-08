using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorGen : MonoBehaviour
{
    public LevelGen level;

    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

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


    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateFloors();
        UpdateMesh();
    }

    
    void CreateFloors()
    {
        for (int x = 0; x < level.tilesX; x++)
        {
            for (int z = 0; z < level.tilesZ; z++)
            {
                if (x < level.tilesX - 1 && z < level.tilesZ - 1)
                {
                    Square square = new Square(level.map[x, z].position, level.tileSize);
                    string state = level.GetState(level.map[x, z].on, level.map[x, z + 1].on, level.map[x + 1, z].on, level.map[x + 1, z + 1].on);
                    if (state == "1000")
                    {
                        CreateTri(square.bottomLeft, square.centerLeft, square.centerBottom);
                    }
                    if (state == "0100")
                    {
                        CreateTri(square.topLeft, square.centerTop, square.centerLeft);
                    }
                    if (state == "0010")
                    {
                        CreateTri(square.bottomRight, square.centerBottom, square.centerRight);
                    }
                    if (state == "0001")
                    {
                        CreateTri(square.topRight, square.centerRight, square.centerTop);
                    }

                    if (state == "1100")
                    {
                        CreateQuad(square.bottomLeft, square.topLeft, square.centerBottom, square.centerTop);
                    }
                    if (state == "0011")
                    {
                        CreateQuad(square.centerBottom, square.centerTop, square.bottomRight, square.topRight);
                    }
                    if (state == "1001")
                    {
                        CreateTri(square.bottomLeft, square.centerLeft, square.centerBottom);
                        CreateTri(square.topRight, square.centerRight, square.centerTop);
                    }
                    if (state == "0110")
                    {
                        CreateTri(square.topLeft, square.centerTop, square.centerLeft);
                        CreateTri(square.bottomRight, square.centerBottom, square.centerRight);
                    }
                    if (state == "0101")
                    {
                        CreateQuad(square.centerLeft, square.topLeft, square.centerRight, square.topRight);
                    }
                    if (state == "1010")
                    {
                        CreateQuad(square.bottomLeft, square.centerLeft, square.bottomRight, square.centerRight);
                    }


                    if (state == "0111")
                    {
                        CreateTri(square.topLeft, square.centerTop, square.centerLeft);
                        CreateTri(square.bottomRight, square.centerBottom, square.centerRight);
                        CreateTri(square.topRight, square.centerRight, square.centerTop);

                        CreateTri(square.centerTop, square.centerRight, square.centerLeft);
                        CreateTri(square.centerBottom, square.centerLeft, square.centerRight);

                    }
                    if (state == "1011")
                    {
                        CreateTri(square.bottomLeft, square.centerLeft, square.centerBottom);
                        CreateTri(square.bottomRight, square.centerBottom, square.centerRight);
                        CreateTri(square.topRight, square.centerRight, square.centerTop);

                        CreateTri(square.centerTop, square.centerRight, square.centerLeft);
                        CreateTri(square.centerBottom, square.centerLeft, square.centerRight);
                    }
                    if (state == "1101")
                    {
                        CreateTri(square.bottomLeft, square.centerLeft, square.centerBottom);
                        CreateTri(square.topLeft, square.centerTop, square.centerLeft);
                        CreateTri(square.topRight, square.centerRight, square.centerTop);

                        CreateTri(square.centerTop, square.centerRight, square.centerLeft);
                        CreateTri(square.centerBottom, square.centerLeft, square.centerRight);
                    }
                    if (state == "1110")
                    {
                        CreateTri(square.bottomLeft, square.centerLeft, square.centerBottom);
                        CreateTri(square.topLeft, square.centerTop, square.centerLeft);
                        CreateTri(square.bottomRight, square.centerBottom, square.centerRight);

                        CreateTri(square.centerTop, square.centerRight, square.centerLeft);
                        CreateTri(square.centerBottom, square.centerLeft, square.centerRight);
                    }

                    if (state == "1111") CreateQuad(square.bottomLeft, square.topLeft, square.bottomRight, square.topRight);
                }

            }
        }
    }

}
