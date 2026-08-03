using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    World world;
    public bool useGPU = false;
    bool generated = false;
    int buffer = 0;
    [SerializeField] List<float> map = new List<float>();
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    Mesh mesh;

    void Start()
    {
        if(!generated) Generate();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void TeraForm(RaycastHit hit, float amount)
    {
        Vector3 pos = transform.InverseTransformPoint(hit.point);
        Vector3Int posRounded = new Vector3Int();
        posRounded.x = Mathf.RoundToInt(pos.x);
        posRounded.y = Mathf.RoundToInt(pos.y);
        posRounded.z = Mathf.RoundToInt(pos.z);
        int i = VoxelHelper.PositionToIndex(posRounded, world.chunkSize);

        if(map[i] <= 0)
        {
            pos = transform.InverseTransformPoint(hit.point - hit.normal / 2);
            posRounded.x = Mathf.RoundToInt(pos.x);
            posRounded.y = Mathf.RoundToInt(pos.y);
            posRounded.z = Mathf.RoundToInt(pos.z);
            i = VoxelHelper.PositionToIndex(posRounded,world.chunkSize);
            map[i] -= amount;
        }
        else
        {
            map[i] -= amount;
        }
    }

    public void Generate()
    {
        world = transform.parent.GetComponent<World>();
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        map.Clear();
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        buffer = 0;
        generated = false;

        for(int i = world.chunkSize * world.chunkSize * world.chunkSize; i > 0; i--)
        {
            Vector3Int position = VoxelHelper.IndexToPosition(i,world.chunkSize);
            map.Add(world.GetValue(transform.position + position));
            
            float[] values = {
            world.GetValue(transform.position + position - new Vector3Int(0,0,1)),
            world.GetValue(transform.position + position - new Vector3Int(1,0,1)),
            world.GetValue(transform.position + position - new Vector3Int(1,0,0)),
            world.GetValue(transform.position + position),
            world.GetValue(transform.position + position - new Vector3Int(0,1,1)),
            world.GetValue(transform.position + position - new Vector3Int(1,1,1)),
            world.GetValue(transform.position + position - new Vector3Int(1,1,0)),
            world.GetValue(transform.position + position - new Vector3Int(0,1,0)) };


            int state = VoxelHelper.GetState(values,world.isoLevel);

            Vector3[] triVerts = new Vector3[3];
            int triIndex = 0;

            int[] triangulation = MarchingCubesTables.triTable[state];

            foreach (int edgeIndex in triangulation)
            {
                if (edgeIndex != -1)
                {
                    int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                    int b = MarchingCubesTables.edgeConnections[edgeIndex][1];

                    Vector3 vertex = VoxelHelper.LerpPoint(values[a], values[b], position - MarchingCubesTables.cubeCorners[a], position - MarchingCubesTables.cubeCorners[b], world.isoLevel);
                    
                    vertices.Add(vertex);
                    triangles.Add(buffer);

                    if (triIndex == 0)
                    {
                        triVerts[0] = vertex;
                        triIndex++;
                    }
                    else if (triIndex == 1)
                    {
                        triVerts[1] = vertex;
                        triIndex++;
                    }
                    else if (triIndex == 2)
                    {
                        triVerts[2] = vertex;
                        uvs.AddRange(VoxelHelper.GetUVs(triVerts[0], triVerts[1], triVerts[2], 1));
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

        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        generated = true;
    }
}
