using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class LevelMeshChunk : MonoBehaviour
{
    public Vector3Int index;
    public LevelMeshGenerator dungeon;
    public Vector3Int chunkSize = new Vector3Int();
    int buffer = 0;
    public float[,,] grid;
    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    Mesh mesh;

    [HideInInspector] public bool generated = false;
    
    public void GenerateChunk()
    {
        mesh = new Mesh();
        if(!dungeon.splitWorldIntoChunks) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;


        if (dungeon.style == LevelMeshGenerator.ArtStyle.BLOCKY)
        {
            for (int x = 0; x < chunkSize.x; x++)
            {
                for (int y = 0; y < chunkSize.y; y++)
                {
                    for (int z = 0; z < chunkSize.z; z++)
                    {
                        if (grid[x, y, z] > dungeon.isoLevel)
                        {
                            if (y > 0)
                            {
                                if (grid[x, y - 1, z] <= dungeon.isoLevel)
                                {
                                    DrawQuadBottom(new Vector3(x, y, z) * dungeon.voxelSize);
                                }
                            }
                            if (y < chunkSize.y - 1)
                            {
                                if (grid[x, y + 1, z] <= dungeon.isoLevel)
                                {
                                    DrawQuadTop(new Vector3(x, y, z) * dungeon.voxelSize);
                                }
                            }
                            if (x > 0)
                            {
                                if (grid[x - 1, y, z] <= dungeon.isoLevel)
                                {
                                    DrawQuadLeft(new Vector3(x, y, z) * dungeon.voxelSize);
                                }
                            }
                            if (x < chunkSize.x - 1)
                            {
                                if (grid[x + 1, y, z] <= dungeon.isoLevel)
                                {
                                    DrawQuadRight(new Vector3(x, y, z) * dungeon.voxelSize);
                                }
                            }
                            if (z > 0)
                            {
                                if (grid[x, y, z - 1] <= dungeon.isoLevel)
                                {
                                    DrawQuadBack(new Vector3(x, y, z) * dungeon.voxelSize);
                                }
                            }
                            if (z < chunkSize.z - 1)
                            {
                                if (grid[x, y, z + 1] <= dungeon.isoLevel)
                                {
                                    DrawQuadFront(new Vector3(x, y, z) * dungeon.voxelSize);
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            for (int x = 0; x < chunkSize.x; x++)
            {
                for (int y = 0; y < chunkSize.y; y++)
                {
                    for (int z = 0; z < chunkSize.z; z++)
                    {
                        if(index.x == dungeon.numberOfChunks.x - 1 && x == chunkSize.x - 1 || index.y == dungeon.numberOfChunks.y - 1 && y == chunkSize.y-1|| index.z == dungeon.numberOfChunks.z - 1 && z == chunkSize.z-1)
                        {
                            continue;
                        }

                        float[] values = new float[]
                        {
                            grid[x,y,z+1],
                            grid[x+1,y,z+1],
                            grid[x+1,y,z],
                            grid[x,y,z],
                            grid[x,y+1,z+1],
                            grid[x+1,y+1,z+1],
                            grid[x+1,y+1,z],
                            grid[x,y+1,z],
                        };

                        Vector3[] points = new Vector3[]
                        {
                            new Vector3(x,y,z+1) * dungeon.voxelSize,
                            new Vector3(x+1,y,z+1) * dungeon.voxelSize,
                            new Vector3(x+1,y,z) * dungeon.voxelSize,
                            new Vector3(x,y,z) * dungeon.voxelSize,
                            new Vector3(x,y+1,z+1) * dungeon.voxelSize,
                            new Vector3(x+1,y+1,z+1) * dungeon.voxelSize,
                            new Vector3(x+1,y+1,z) * dungeon.voxelSize,
                            new Vector3(x,y+1,z) * dungeon.voxelSize,
                        };

                        int cubeIndex = VoxelHelper.GetState(values, dungeon.isoLevel);

                        Vector3[] triVerts = new Vector3[3];
                        int triIndex = 0;

                        int[] triangulation = MarchingCubesTables.triTable[cubeIndex];
                        foreach (int edgeIndex in triangulation)
                        {
                            if (edgeIndex > -1)
                            {
                                int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                                int b = MarchingCubesTables.edgeConnections[edgeIndex][1];

                                Vector3 vertexPos = Vector3.Lerp(points[a], points[b], 0.5f);
                                if(dungeon.style == LevelMeshGenerator.ArtStyle.SMOOTH) vertexPos = VoxelHelper.LerpPoint(values[a], values[b], points[a], points[b], dungeon.isoLevel);
                                
                                verts.Add(vertexPos);
                                tris.Add(buffer);

                                if (triIndex == 0)
                                {
                                    triVerts[0] = vertexPos;
                                    triIndex++;
                                }
                                else if (triIndex == 1)
                                {
                                    triVerts[1] = vertexPos;
                                    triIndex++;
                                }
                                else if (triIndex == 2)
                                {
                                    triVerts[2] = vertexPos;
                                    uvs.AddRange(VoxelHelper.GetUVs(triVerts[0], triVerts[1], triVerts[2], dungeon.voxelSize));
                                    triIndex = 0;
                                }

                                buffer++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }


        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
        
        if(MeshDataExists())
        {
            generated = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool MeshDataExists()
    {
        if(mesh)
        {
            if(mesh.vertices.Length > 0) return true;
        }
        return false;
    }
    
    public bool PlacePlayer()
    {
        if(!dungeon.player || !generated || !MeshDataExists()) return false;
        for(int x = 0; x < chunkSize.x; x++)
        {
            for(int y = 0; y < chunkSize.y; y++)
            {
                for(int z = 0; z < chunkSize.z; z++)
                {
                    dungeon.player.position = transform.position + (new Vector3(x,chunkSize.y,z) * dungeon.voxelSize);
                    if(Physics.Raycast(dungeon.player.position, Vector3.down, out RaycastHit hit))
                    {
                        dungeon.player.position = hit.point;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void DrawQuadTop(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) *  dungeon.voxelSize + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) *   dungeon.voxelSize + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) *  dungeon.voxelSize + position);
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * dungeon.voxelSize + position);

        tris.Add(buffer + 2);
        tris.Add(buffer + 1);
        tris.Add(buffer + 0);

        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 3);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadFront(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) *  dungeon.voxelSize + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) *   dungeon.voxelSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) *  dungeon.voxelSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * dungeon.voxelSize + position);

        tris.Add(buffer + 0);
        tris.Add(buffer + 1);
        tris.Add(buffer + 2);

        tris.Add(buffer + 3);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadBack(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) *  dungeon.voxelSize + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) *   dungeon.voxelSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) *  dungeon.voxelSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * dungeon.voxelSize + position);

        tris.Add(buffer + 2);
        tris.Add(buffer + 1);
        tris.Add(buffer + 0);

        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 3);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadLeft(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) *  dungeon.voxelSize + position);
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) *   dungeon.voxelSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) *  dungeon.voxelSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * dungeon.voxelSize + position);

        tris.Add(buffer + 0);
        tris.Add(buffer + 1);
        tris.Add(buffer + 2);

        tris.Add(buffer + 3);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadRight(Vector3 position)
    {
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) *  dungeon.voxelSize + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) *   dungeon.voxelSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) *  dungeon.voxelSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * dungeon.voxelSize + position);

        tris.Add(buffer + 2);
        tris.Add(buffer + 1);
        tris.Add(buffer + 0);

        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 3);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadBottom(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) *  dungeon.voxelSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) *   dungeon.voxelSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) *  dungeon.voxelSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * dungeon.voxelSize + position);

        tris.Add(buffer + 0);
        tris.Add(buffer + 1);
        tris.Add(buffer + 2);
        tris.Add(buffer + 3);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

}
