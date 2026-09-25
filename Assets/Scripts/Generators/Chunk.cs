using UnityEngine;
using System.Collections.Generic;

public class Chunk : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer renderer;
    public MeshCollider collider;
    public Vector3Int index;
    public LevelMeshGenerator generator;
    [HideInInspector] public float[] grid;
    public Vector3Int offset;

    public static int numThreads = 8;
    public ComputeShader marchingShader;
    ComputeBuffer trianglesBuffer;
    ComputeBuffer triangleCountBuffer;
    ComputeBuffer weightsBuffer;

    void OnValidate()
    {
        if(meshFilter.sharedMesh) Generate();
    }

    public void Generate()
    {
        CreateBuffers();

        if(generator.type == LevelType.DUNGEON)
        {
            grid = generator.dungeonGrid;
        }
        else
        {
            Game.get = GameObject.Find("GameManager").GetComponent<Game>();
            Game.get.noise.generator = generator;
            grid = Game.get.noise.GetNoise(index);
        }
        
        meshFilter.mesh = collider.sharedMesh = ConstructMesh();
        ReleaseBuffers();
    }

    void CreateBuffers()
    {
        trianglesBuffer = new ComputeBuffer(5 * generator.chunkSize.x * generator.chunkSize.y * generator.chunkSize.z, Triangle.SizeOf, ComputeBufferType.Append);
        if(generator.style == LevelStyle.BLOCKY) trianglesBuffer = new ComputeBuffer(12 * generator.chunkSize.x * generator.chunkSize.y * generator.chunkSize.z, Triangle.SizeOf, ComputeBufferType.Append);
        triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        weightsBuffer = new ComputeBuffer(generator.chunkSize.x * generator.chunkSize.y * generator.chunkSize.z, sizeof(float));
    }

    void ReleaseBuffers()
    {
        trianglesBuffer.Release();
        triangleCountBuffer.Release();
        weightsBuffer.Release();
    }

    int ReadTriangleCount()
    {
        int[] triCount = {0};
        ComputeBuffer.CopyCount(trianglesBuffer, triangleCountBuffer, 0);
        triangleCountBuffer.GetData(triCount);
        return triCount[0];
    }

    Mesh CreateMeshFromTriangles(Triangle[] triangles, bool insideOut = false)
    {
        Vector3[] vertices = new Vector3[triangles.Length * 3];
        int[] indices = new int[triangles.Length * 3];
        List<Vector2> uvs = new List<Vector2>();
        Vector3 offset = new Vector3(index.x * (generator.chunkSize.x-1), index.y * (generator.chunkSize.y-1), index.z *  (generator.chunkSize.z-1));

        if(insideOut)
        {
            for (int i = 0; i < triangles.Length; i++) 
            {
                int startIndex = i * 3; 
                vertices[startIndex] = triangles[i].a + offset;
                vertices[startIndex + 1] = triangles[i].b + offset;
                vertices[startIndex + 2] = triangles[i].c + offset; 
                indices[startIndex] = startIndex;
                indices[startIndex + 1] = startIndex + 1;
                indices[startIndex + 2] = startIndex + 2;
                uvs.AddRange(VoxelHelper.GetUVs(triangles[i].a, triangles[i].b, triangles[i].c));
            }
        }
        else
        {
            for (int i = 0; i < triangles.Length; i++) 
            {
                int startIndex = i * 3; 
                vertices[startIndex] = triangles[i].c + offset;
                vertices[startIndex + 1] = triangles[i].b + offset;
                vertices[startIndex + 2] = triangles[i].a + offset; 
                indices[startIndex] = startIndex;
                indices[startIndex + 1] = startIndex + 1;
                indices[startIndex + 2] = startIndex + 2;
                uvs.AddRange(VoxelHelper.GetUVs(triangles[i].c, triangles[i].b, triangles[i].a));
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    Mesh ConstructMesh()
    {
        marchingShader.SetBuffer(0,"triangles", trianglesBuffer);
        marchingShader.SetBuffer(0, "weights", weightsBuffer);
        int[] chunkSize = {generator.chunkSize.x, generator.chunkSize.y, generator.chunkSize.z};
        marchingShader.SetInts("chunkSize", chunkSize);
        marchingShader.SetFloat("isoLevel", generator.isoLevel);
        marchingShader.SetInt("style", (int)generator.style);
        weightsBuffer.SetData(grid);
        trianglesBuffer.SetCounterValue(0);

        marchingShader.Dispatch(0, generator.chunkSize.x / numThreads, generator.chunkSize.y / numThreads, generator.chunkSize.z / numThreads);
        Triangle[] triangles = new Triangle[ReadTriangleCount()];
        
        trianglesBuffer.GetData(triangles);
        return CreateMeshFromTriangles(triangles);

    }
}
