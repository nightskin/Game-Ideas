using UnityEngine;

public class NoiseGPU : MonoBehaviour
{
    public ComputeShader noiseShader;
    [SerializeField] float noiseScale = 1f;
    [SerializeField] float amplitude = 5f;
    [SerializeField] float frequency = 0.005f;
    [SerializeField] int octaves = 8;
    [SerializeField, Range(0f, 1f)] float groundPercent = 0.2f;
    ComputeBuffer weightsBuffer;

    void CreateBuffers()
    {
        weightsBuffer = new ComputeBuffer(LevelMeshChunk.chunkSize * LevelMeshChunk.chunkSize * LevelMeshChunk.chunkSize, sizeof(float));
    }

    void ReleaseBuffers()
    {
        weightsBuffer.Release();
    }

    public float[] GetNoise()
    {
        CreateBuffers();

        float[] weights = new float[LevelMeshChunk.chunkSize * LevelMeshChunk.chunkSize * LevelMeshChunk.chunkSize];
        noiseShader.SetBuffer(0, "weights", weightsBuffer);
        
        noiseShader.SetInt("chunkSize",LevelMeshChunk.chunkSize);
        noiseShader.SetFloat("noiseScale",noiseScale);
        noiseShader.SetFloat("amplitude", amplitude);
        noiseShader.SetFloat("frequency", frequency);
        noiseShader.SetInt("octaves", octaves);
        noiseShader.SetFloat("groundPercent", groundPercent);
        
        noiseShader.Dispatch(0, LevelMeshChunk.chunkSize / LevelMeshChunk.numThreads, LevelMeshChunk.chunkSize / LevelMeshChunk.numThreads, LevelMeshChunk.chunkSize / LevelMeshChunk.numThreads);
        weightsBuffer.GetData(weights);
        ReleaseBuffers();
        return weights;
    }
}
