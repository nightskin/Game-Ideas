using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    public static int resolution = 8;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;
    Mesh mesh;

    public void CreateMesh()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        Vector3 center = new Vector3(resolution - 1, resolution - 1, resolution - 1) / 2 * Voxel.size;

        for(int x = 0; x < resolution - 1; x++)
        {
            for(int y = 0; y < resolution - 1; y++)
            {
                for(int z = 0; z < resolution - 1; z++)
                {
                    Vector3[] points = new Vector3[]
                    {
                        transform.position + (new Vector3(x,y,z+1) * Voxel.size) - center,
                        transform.position + (new Vector3(x+1,y,z+1) * Voxel.size) - center,
                        transform.position + (new Vector3(x+1,y,z) * Voxel.size) - center,
                        transform.position + (new Vector3(x,y,z) * Voxel.size) - center,
                        transform.position + (new Vector3(x,y+1,z+1) * Voxel.size) - center,
                        transform.position + (new Vector3(x+1,y+1,z+1) * Voxel.size) - center,
                        transform.position + (new Vector3(x+1,y+1,z) * Voxel.size) - center,
                        transform.position + (new Vector3(x,y+1,z) * Voxel.size) - center,
                    };
                    float[] values = new float[]
                    {
                        World.Evaluate(points[0]),
                        World.Evaluate(points[1]),
                        World.Evaluate(points[2]),
                        World.Evaluate(points[3]),
                        World.Evaluate(points[4]),
                        World.Evaluate(points[5]),
                        World.Evaluate(points[6]),
                        World.Evaluate(points[7]),
                    };

                    int cubeIndex = Voxel.GetState(values);

                    Vector3[] triVerts = new Vector3[3];
                    int triIndex = 0;

                    int[] triangulation = MarchingCubesTables.triTable[cubeIndex];
                    foreach (int edgeIndex in triangulation)
                    {
                        if(edgeIndex > -1)
                        {
                                int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                                int b = MarchingCubesTables.edgeConnections[edgeIndex][1];

                                Vector3 vertexPos = Voxel.LerpPoint(values[a], values[b], points[a], points[b]);
                                
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
                                    uvs.AddRange(Voxel.GetUVs(triVerts[0], triVerts[1], triVerts[2]));
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
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
