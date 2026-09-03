using UnityEngine;
using System.Collections.Generic;

public class LevelMeshChunkGPU : LevelMeshChunk
{
    public ComputeShader marchingShader;
    struct Triangle 
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public static int SizeOf => sizeof(float) * 3 * 3;
    }
    ComputeBuffer trianglesBuffer;
    ComputeBuffer triangleCountBuffer;
    ComputeBuffer weightsBuffer;

    
    public override void Generate()
    {
        CreateBuffers();
        Game.get = GameObject.Find("GameManager").GetComponent<Game>();
        grid = Game.get.noise.GetNoise();
        meshFilter.mesh = collider.sharedMesh = ConstructMesh();
        ReleaseBuffers();
    }

    void CreateBuffers()
    {
        trianglesBuffer = new ComputeBuffer(5 * chunkSize * chunkSize * chunkSize, Triangle.SizeOf, ComputeBufferType.Append);
        triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        weightsBuffer = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(float));
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

    Mesh CreateMeshFromTriangles(Triangle[] triangles)
    {
        Vector3[] vertices = new Vector3[triangles.Length * 3];
        int[] indices = new int[triangles.Length * 3];
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < triangles.Length; i++) 
        {
            int startIndex = i * 3; 
            vertices[startIndex] = triangles[i].a;
            vertices[startIndex + 1] = triangles[i].b;
            vertices[startIndex + 2] = triangles[i].c; 
            indices[startIndex] = startIndex;
            indices[startIndex + 1] = startIndex + 1;
            indices[startIndex + 2] = startIndex + 2;
            uvs.AddRange(VoxelHelper.GetUVs(triangles[i].a, triangles[i].b, triangles[i].c));
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        return mesh;
    }

    Mesh ConstructMesh()
    {
        marchingShader.SetBuffer(0,"triangles", trianglesBuffer);
        marchingShader.SetBuffer(0, "weights", weightsBuffer);
        marchingShader.SetInt("chunkSize", chunkSize);
        marchingShader.SetFloat("isoLevel", .5f);

        weightsBuffer.SetData(grid);
        trianglesBuffer.SetCounterValue(0);

        marchingShader.Dispatch(0, chunkSize / numThreads, chunkSize / numThreads, chunkSize / numThreads);
        Triangle[] triangles = new Triangle[ReadTriangleCount()];
        
        trianglesBuffer.GetData(triangles);
        return CreateMeshFromTriangles(triangles);
    }
}
