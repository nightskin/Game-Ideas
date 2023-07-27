using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGen : MonoBehaviour
{
    public LevelGen level;

    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //Create Walls
        if(!level.threeDimensions)
        {
            for (int x = 0; x < level.tilesX; x++)
            {
                for (int z = 0; z < level.tilesZ; z++)
                {
                    if (level.map[x, z].on)
                    {
                        if (x == 0)
                        {
                            CreateWalls(new Vector3(x, 0, z) * level.stepSize, Vector3.one * level.stepSize / 2, false, false, true);
                        }
                        if (x > 0)
                        {
                            if (!level.map[x - 1, z].on)
                            {
                                CreateWalls(new Vector3(x, 0, z) * level.stepSize, Vector3.one * level.stepSize / 2, false, false, true);
                            }
                        }

                        if (x < level.tilesX - 1)
                        {
                            if (!level.map[x + 1, z].on)
                            {
                                CreateWalls(new Vector3(x, 0, z) * level.stepSize, Vector3.one * level.stepSize / 2, false, false, false, true);
                            }
                        }
                        if (x == level.tilesX - 1)
                        {
                            CreateWalls(new Vector3(x, 0, z) * level.stepSize, Vector3.one * level.stepSize / 2, false, false, false, true);
                        }

                        if (z == 0)
                        {
                            CreateWalls(new Vector3(x, 0, z) * level.stepSize, Vector3.one * level.stepSize / 2, false, true);
                        }
                        if (z > 0)
                        {
                            if (!level.map[x, z - 1].on)
                            {
                                CreateWalls(new Vector3(x, 0, z) * level.stepSize, Vector3.one * level.stepSize / 2, false, true);
                            }
                        }

                        if (z < level.tilesZ - 1)
                        {
                            if (!level.map[x, z + 1].on)
                            {
                                CreateWalls(new Vector3(x, 0, z) * level.stepSize, Vector3.one * level.stepSize / 2, true);
                            }
                        }
                        if (z == level.tilesX - 1)
                        {
                            CreateWalls(new Vector3(x, 0, z) * level.stepSize, Vector3.one * level.stepSize / 2, true);
                        }

                    }
                }
            }
        }
        else
        {
            for (int x = 0; x < level.tilesX; x++)
            {
                for(int y = 0; y < level.tilesY; y++)
                {
                    for (int z = 0; z < level.tilesZ; z++)
                    {
                        if (level.map3d[x,y,z].on)
                        {
                            if (x == 0)
                            {
                                CreateWalls(new Vector3(x, y, z) * level.stepSize, Vector3.one * level.stepSize / 2, false, false, true);
                            }
                            if (x > 0)
                            {
                                if (!level.map3d[x - 1, y,z].on)
                                {
                                    CreateWalls(new Vector3(x, y, z) * level.stepSize, Vector3.one * level.stepSize / 2, false, false, true);
                                }
                            }

                            if (x < level.tilesX - 1)
                            {
                                if (!level.map3d[x + 1,y, z].on)
                                {
                                    CreateWalls(new Vector3(x, y, z) * level.stepSize, Vector3.one * level.stepSize / 2, false, false, false, true);
                                }
                            }
                            if (x == level.tilesX - 1)
                            {
                                CreateWalls(new Vector3(x, y, z) * level.stepSize, Vector3.one * level.stepSize / 2, false, false, false, true);
                            }

                            if (z == 0)
                            {
                                CreateWalls(new Vector3(x, y, z) * level.stepSize, Vector3.one * level.stepSize / 2, false, true);
                            }
                            if (z > 0)
                            {
                                if (!level.map3d[x, y,z - 1].on)
                                {
                                    CreateWalls(new Vector3(x, y, z) * level.stepSize, Vector3.one * level.stepSize / 2, false, true);
                                }
                            }

                            if (z < level.tilesZ - 1)
                            {
                                if (!level.map3d[x, y,z + 1].on)
                                {
                                    CreateWalls(new Vector3(x, y, z) * level.stepSize, Vector3.one * level.stepSize / 2, true);
                                }
                            }
                            if (z == level.tilesX - 1)
                            {
                                CreateWalls(new Vector3(x, y, z) * level.stepSize, Vector3.one * level.stepSize / 2, true);
                            }
                        }
                    }
                }

            }
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

    void CreateQuad(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        verts.Add(Quaternion.Euler(rotation) * new Vector3(-1 * scale.x, -1 * scale.y, -1 * scale.z) + position);
        verts.Add(Quaternion.Euler(rotation) * new Vector3(-1 * scale.x, -1 * scale.y, 1 * scale.z) + position);
        verts.Add(Quaternion.Euler(rotation) * new Vector3(1 * scale.x, -1 * scale.y, -1 * scale.z) + position);
        verts.Add(Quaternion.Euler(rotation) * new Vector3(1 * scale.x, -1 * scale.y, 1 * scale.z) + position);

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

    public void CreateWalls(Vector3 offset, Vector3 size, bool front = false, bool back = false, bool left = false, bool right = false, bool up = false, bool down = false)
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
        if (up)
            CreateQuad(new Vector3(-1 * size.x, 1 * size.y, 1 * size.z), new Vector3(-1 * size.x, 1 * size.y, -1 * size.z), new Vector3(1 * size.x, 1 * size.y, 1 * size.z), new Vector3(1 * size.x, 1 * size.y, -1 * size.z), offset);

        if (down)
            CreateQuad(new Vector3(-1 * size.x, -1 * size.y, -1 * size.z), new Vector3(-1 * size.x, -1 * size.y, 1 * size.z), new Vector3(1 * size.x, -1 * size.y, -1 * size.z), new Vector3(1 * size.x, -1 * size.y, 1 * size.z), offset);

    }


    public void UpdateMesh()
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
