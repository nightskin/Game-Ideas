using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public Vector3Int id;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<Color> colors = new List<Color>();
    List<int> tris = new List<int>();
    int buffer = 0;
    Mesh mesh;

    public void Generate()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        for(int x = World.get.chunkSize.x; x > 0; x--)
        {
            for(int z = World.get.chunkSize.z; z > 0; z--)
            {
                for(int y = World.get.chunkSize.y; y > 0; y--)
                {
                    Vector3[] corners = new Vector3[]
                    {
                        new Vector3(x,y,z-1) * World.get.voxelSize,
                        new Vector3(x-1,y,z-1) * World.get.voxelSize,
                        new Vector3(x-1,y,z) * World.get.voxelSize,
                        new Vector3(x,y,z) * World.get.voxelSize,
                        new Vector3(x,y-1,z-1) * World.get.voxelSize,
                        new Vector3(x-1,y-1,z-1) * World.get.voxelSize,
                        new Vector3(x-1,y-1,z) * World.get.voxelSize,
                        new Vector3(x,y-1,z) * World.get.voxelSize,
                    };
                    float[] values = new float[]
                    {
                        World.get.GetValue(corners[0] + transform.position, new Vector3Int(x,y,z-1)),
                        World.get.GetValue(corners[1] + transform.position, new Vector3Int(x-1,y,z-1)),
                        World.get.GetValue(corners[2] + transform.position, new Vector3Int(x-1,y,z)),
                        World.get.GetValue(corners[3] + transform.position, new Vector3Int(x,y,z)),
                        World.get.GetValue(corners[4] + transform.position, new Vector3Int(x,y-1,z-1)),
                        World.get.GetValue(corners[5] + transform.position, new Vector3Int(x-1,y-1,z-1)),
                        World.get.GetValue(corners[6] + transform.position, new Vector3Int(x-1,y-1,z)),
                        World.get.GetValue(corners[7] + transform.position, new Vector3Int(x,y-1,z)),
                    };

                    int cubeIndex = VoxelHelper.GetState(values, World.get.isoLevel);

                    Vector3[] triVerts = new Vector3[3];
                    int triIndex = 0;

                    int[] triangulation = MarchingCubesTables.triTable[cubeIndex];
                    foreach (int edgeIndex in triangulation)
                    {
                        if(edgeIndex > -1)
                        {
                                int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                                int b = MarchingCubesTables.edgeConnections[edgeIndex][1];

                                Vector3 vertexPos = VoxelHelper.LerpPoint(values[a], values[b], corners[a], corners[b],World.get.isoLevel);
                                Color color = World.get.GetColor(vertexPos);
                                
                                verts.Add(vertexPos);
                                tris.Add(buffer);
                                colors.Add(color);

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
                                    uvs.AddRange(VoxelHelper.GetUVs(triVerts[0], triVerts[1], triVerts[2], World.get.voxelSize));
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

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris,0);
        mesh.SetColors(colors);
        mesh.SetUVs(0,uvs);

        mesh.Optimize();
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
