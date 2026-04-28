using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;
    Mesh mesh;

    public void Generate()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        for(int x = 0; x < World.get.chunkSize.x; x++)
        {
            for(int z = 0; z < World.get.chunkSize.z; z++)
            {
                for(int y = 0; y < World.get.chunkSize.y; y++)
                {
                    Vector3[] corners = new Vector3[]
                    {
                        new Vector3(x,y,z+1),
                        new Vector3(x+1,y,z+1),
                        new Vector3(x+1,y,z),
                        new Vector3(x,y,z),
                        new Vector3(x,y+1,z+1),
                        new Vector3(x+1,y+1,z+1),
                        new Vector3(x+1,y+1,z),
                        new Vector3(x,y+1,z),
                    };
                    float[] values = new float[]
                    {
                        World.get.Evaluate(corners[0] + transform.position),
                        World.get.Evaluate(corners[1] + transform.position),
                        World.get.Evaluate(corners[2] + transform.position),
                        World.get.Evaluate(corners[3] + transform.position),
                        World.get.Evaluate(corners[4] + transform.position),
                        World.get.Evaluate(corners[5] + transform.position),
                        World.get.Evaluate(corners[6] + transform.position),
                        World.get.Evaluate(corners[7] + transform.position),
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

                                Vector3 vertexPos = Voxel.LerpPoint(values[a], values[b], corners[a], corners[b]);
                                
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
        mesh.SetVertices(verts.ToArray());
        mesh.SetTriangles(tris.ToArray(),0);
        mesh.SetUVs(0,uvs.ToArray());

        mesh.Optimize();
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
