using UnityEngine;
using System.Collections.Generic;

public class LevelMeshChunkCPU : LevelMeshChunk
{
    int buffer = 0;
    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    Mesh mesh;

    public override void Generate()
    {
        grid = new float[(int)Mathf.Pow(chunkSize+1,3)];
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh = mesh;

        if (style == ArtStyle.BLOCKY)
            {
                for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        if (grid[(VoxelHelper.Index3dToIndex(new Vector3Int(x,y,z),chunkSize))] > generator.isoLevel)
                        {
                            if (y > 0)
                            {
                                if (grid[(VoxelHelper.Index3dToIndex(new Vector3Int(x,y-1,z),chunkSize))] <= generator.isoLevel)
                                {
                                    DrawQuadBottom(new Vector3(x, y, z));
                                }
                            }
                            if (y < chunkSize - 1)
                            {
                                if (grid[(VoxelHelper.Index3dToIndex(new Vector3Int(x,y+1,z),chunkSize))] <= generator.isoLevel)
                                {
                                    DrawQuadTop(new Vector3(x, y, z));
                                }
                            }
                            if (x > 0)
                            {
                                if (grid[(VoxelHelper.Index3dToIndex(new Vector3Int(x-1,y,z),chunkSize))] <= generator.isoLevel)
                                {
                                    DrawQuadLeft(new Vector3(x, y, z));
                                }
                            }
                            if (x < chunkSize - 1)
                            {
                                if (grid[(VoxelHelper.Index3dToIndex(new Vector3Int(x+1,y,z),chunkSize))] <= generator.isoLevel)
                                {
                                    DrawQuadRight(new Vector3(x, y, z));
                                }
                            }
                            if (z > 0)
                            {
                                if (grid[(VoxelHelper.Index3dToIndex(new Vector3Int(x,y,z-1),chunkSize))] <= generator.isoLevel)
                                {
                                    DrawQuadBack(new Vector3(x, y, z));
                                }
                            }
                            if (z < chunkSize - 1)
                            {
                                if (grid[(VoxelHelper.Index3dToIndex(new Vector3Int(x,y,z+1),chunkSize))] <= generator.isoLevel)
                                {
                                    DrawQuadFront(new Vector3(x, y, z));
                                }
                            }
                        }
                    }
                }
            }
            }
        else
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        if(index.x == generator.numberOfChunks - 1 && x == chunkSize - 1)
                        {
                            continue;
                        }
                        if(index.y == generator.numberOfChunks - 1 && y == chunkSize - 1)
                        {
                            continue;
                        }
                        if(index.z == generator.numberOfChunks - 1 && z == chunkSize - 1)
                        {
                            continue;
                        }

                        float[] values = new float[]
                        {
                            generator.GetDungeonValue(index * chunkSize + new Vector3Int(x,y,z+1)),
                            generator.GetDungeonValue(index * chunkSize + new Vector3Int(x+1,y,z+1)),
                            generator.GetDungeonValue(index * chunkSize + new Vector3Int(x+1,y,z)),
                            generator.GetDungeonValue(index * chunkSize + new Vector3Int(x,y,z)),
                            generator.GetDungeonValue(index * chunkSize + new Vector3Int(x,y+1,z+1)), 
                            generator.GetDungeonValue(index * chunkSize + new Vector3Int(x+1,y+1,z+1)),
                            generator.GetDungeonValue(index * chunkSize + new Vector3Int(x+1,y+1,z)),
                            generator.GetDungeonValue(index * chunkSize + new Vector3Int(x,y+1,z)),  
                        };

                        if(levelType == LevelType.CAVES)
                        {
                            values = new float[]
                            {
                                generator.GetCaveValue(index * chunkSize + new Vector3Int(x,y,z+1)),
                                generator.GetCaveValue(index * chunkSize + new Vector3Int(x+1,y,z+1)),
                                generator.GetCaveValue(index * chunkSize + new Vector3Int(x+1,y,z)),
                                generator.GetCaveValue(index * chunkSize + new Vector3Int(x,y,z)),
                                generator.GetCaveValue(index * chunkSize + new Vector3Int(x,y+1,z+1)), 
                                generator.GetCaveValue(index * chunkSize + new Vector3Int(x+1,y+1,z+1)),
                                generator.GetCaveValue(index * chunkSize + new Vector3Int(x+1,y+1,z)),
                                generator.GetCaveValue(index * chunkSize + new Vector3Int(x,y+1,z)),   
                            };
                        }
                        else if(levelType == LevelType.TERRAIN)
                        {
                            values = new float[]
                            {
                                generator.GetTerrainValue(index * chunkSize + new Vector3Int(x,y,z+1)),
                                generator.GetTerrainValue(index * chunkSize + new Vector3Int(x+1,y,z+1)),
                                generator.GetTerrainValue(index * chunkSize + new Vector3Int(x+1,y,z)),
                                generator.GetTerrainValue(index * chunkSize + new Vector3Int(x,y,z)),
                                generator.GetTerrainValue(index * chunkSize + new Vector3Int(x,y+1,z+1)), 
                                generator.GetTerrainValue(index * chunkSize + new Vector3Int(x+1,y+1,z+1)),
                                generator.GetTerrainValue(index * chunkSize + new Vector3Int(x+1,y+1,z)),
                                generator.GetTerrainValue(index * chunkSize + new Vector3Int(x,y+1,z)),   
                            };
                        }

                        Vector3[] points = new Vector3[]
                        {
                            new Vector3(x,y,z+1),
                            new Vector3(x+1,y,z+1) ,
                            new Vector3(x+1,y,z) ,
                            new Vector3(x,y,z) ,
                            new Vector3(x,y+1,z+1),
                            new Vector3(x+1,y+1,z+1) ,
                            new Vector3(x+1,y+1,z) ,
                            new Vector3(x,y+1,z) ,
                        };

                        int cubeIndex = VoxelHelper.GetState(values, generator.isoLevel);

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
                                if(style == ArtStyle.SMOOTH) vertexPos = VoxelHelper.LerpPoint(values[a], values[b], points[a], points[b], generator.isoLevel);
                                
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
                                    uvs.AddRange(VoxelHelper.GetUVs(triVerts[0], triVerts[1], triVerts[2]));
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
        if(mesh.vertexCount >= 3) collider.sharedMesh = mesh;

    }
    
    void DrawQuadTop(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f)    + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f)     + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f)    + position);
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f)   + position);

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
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f)   + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f)     + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f)    + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f)   + position);

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
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f)   + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f)     + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f)    + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f)   + position);

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
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f)    + position);
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f)     + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f)    + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f)   + position);

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
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f)  + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f)   + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f)  + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) + position);

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
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f)    + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f)     + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f)    + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f)   + position);

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
