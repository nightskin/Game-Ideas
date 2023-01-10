using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LevelGenerator : MonoBehaviour
{
    public float threshold = 1;
    public float noise = 0.3f;

    public int[,] map;
    public int sizeX = 10;
    public int sizeZ = 10;

    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    int buffer = 0;

    float normal = 0.005f;


    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateNoise();
        SmoothMap(5);
        CreateMap();
        
        UpdateMesh();
    }

    void Update()
    {
        normal = Mathf.PingPong(Time.time * 0.25f, 0.08f);
        GetComponent<MeshRenderer>().material.SetFloat("_Parallax", normal);
    }

    void CreateQuad(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft)
    {
        vertices.Add(topLeft);
        vertices.Add(topRight);
        vertices.Add(bottomRight);
        vertices.Add(bottomLeft);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));

        triangles.Add(0 + buffer);
        triangles.Add(1 + buffer);
        triangles.Add(2 + buffer);
        triangles.Add(1 + buffer);
        triangles.Add(3 + buffer);
        triangles.Add(2 + buffer);

        buffer += 4;
    }

    void CreateTriangle(Vector3 top, Vector3 lowerRight, Vector3 lowerLeft)
    {
        vertices.Add(top);
        vertices.Add(lowerRight);
        vertices.Add(lowerLeft);

        uvs.Add(new Vector2(0.5f, 0.5f));
        uvs.Add(new Vector2(0,0));
        uvs.Add(new Vector2(1, 0));

        triangles.Add(0 + buffer);
        triangles.Add(1 + buffer);
        triangles.Add(2 + buffer);

        buffer += 3;
    }

    void CreateCube(Vector3 position, Vector3 scale)
    {
        //Top
        CreateQuad(new Vector3(0.5f * scale.x, -0.5f * scale.y, 0.5f * scale.z) + position, new Vector3(-0.5f * scale.x, -0.5f * scale.y, 0.5f * scale.z) + position, new Vector3(0.5f * scale.x, -0.5f * scale.y, -0.5f * scale.z) + position, new Vector3(-0.5f * scale.x, -0.5f * scale.y, -0.5f * scale.z) + position);
        //Botom
        CreateQuad(new Vector3(-0.5f * scale.x, 0.5f * scale.y, 0.5f * scale.z) + position, new Vector3(0.5f * scale.x, 0.5f * scale.y, 0.5f * scale.z) + position, new Vector3(-0.5f * scale.x, 0.5f * scale.y, -0.5f * scale.z) + position, new Vector3(0.5f * scale.x, 0.5f * scale.y, -0.5f * scale.z) + position);

        //Wall Back
        CreateQuad(new Vector3(-0.5f * scale.x, 0.5f * scale.y, -0.5f * scale.z) + position, new Vector3(0.5f * scale.x, 0.5f * scale.y, -0.5f * scale.z) + position, new Vector3(-0.5f * scale.x, -0.5f * scale.y, -0.5f * scale.z) + position, new Vector3(0.5f * scale.x, -0.5f * scale.y, -0.5f * scale.z) + position);
        //Wall Front
        CreateQuad(new Vector3(0.5f * scale.x, 0.5f * scale.y, 0.5f * scale.z) + position, new Vector3(-0.5f * scale.x, 0.5f * scale.y, 0.5f * scale.z) + position, new Vector3(0.5f * scale.x, -0.5f * scale.y, 0.5f * scale.z) + position, new Vector3(-0.5f * scale.x, -0.5f * scale.y, 0.5f * scale.z) + position);

        //Wall Right
        CreateQuad(new Vector3(0.5f * scale.x, 0.5f * scale.y, -0.5f * scale.z) + position, new Vector3(0.5f * scale.x, 0.5f * scale.y, 0.5f * scale.z) + position, new Vector3(0.5f * scale.x, -0.5f * scale.y, -0.5f * scale.z) + position, new Vector3(0.5f * scale.x, -0.5f * scale.y, 0.5f * scale.z) + position);
        //Wall Left
        CreateQuad(new Vector3(-0.5f * scale.x, 0.5f * scale.y, 0.5f * scale.z) + position, new Vector3(-0.5f * scale.x, 0.5f * scale.y, -0.5f * scale.z) + position, new Vector3(-0.5f * scale.x, -0.5f * scale.y, 0.5f * scale.z) + position, new Vector3(-0.5f * scale.x, -0.5f * scale.y, -0.5f * scale.z) + position);
    }

    void CreateNoise()
    {
        map = new int[sizeX,sizeZ];
        for(int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                if(x == 0 || x == sizeX-1 || z == 0 || z == sizeZ-1)
                {
                    map[x, z] = 1;
                }
                else
                {
                    map[x, z] = Random.Range(0, 2);
                }

            }
        }
    }

    void SmoothMap(int iterations)
    {
        for(int i = 0; i < iterations; i++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    int n = GetNeighbours(x, z);
                    if(n > 4)
                    {
                        map[x, z] = 1;
                    }
                    else if(n < 4)
                    {
                        map[x, z] = 0;
                    }
                }
            }
        }
    }

    int GetNeighbours(int xIndex, int zIndex)
    {
        int wallCount = 0;
        for(int x = xIndex - 1; x <= xIndex+1; x++)
        {
            for (int z = zIndex - 1; z <= zIndex + 1; z++)
            {
                if(x >= 0 && x < sizeX && z >= 0 && z < sizeZ)
                {
                    if (x != xIndex || z != zIndex)
                    {
                        wallCount += map[x, z];
                    }
                }
                else
                {
                    wallCount++;
                }

            }
        }

        return wallCount;
    }

    void CreateMap()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int z = 0; z < sizeZ; z++)
            {
                if (map[x, z] == 1)
                {
                    CreateCube(new Vector3(x - (sizeX / 2), 0, z - (sizeZ / 2 )), Vector3.one);
                }
                else
                {
                    CreateCube(new Vector3(x - (sizeX / 2), -1, z - (sizeZ / 2)), Vector3.one);
                }
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }


}
