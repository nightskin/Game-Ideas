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

    void Awake()
    {
        CreateBuffers();
    }
    void OnDestroy()
    {
        ReleaseBuffers();
    }

    void CreateBuffers()
    {
        weightsBuffer = new ComputeBuffer(GridMetrics.pointsPerChunk * GridMetrics.pointsPerChunk * GridMetrics.pointsPerChunk, sizeof(float));
    }

    void ReleaseBuffers()
    {
        weightsBuffer.Release();
    }

    public float[] GetNoise()
    {
        float[] heights = new float[GridMetrics.pointsPerChunk * GridMetrics.pointsPerChunk * GridMetrics.pointsPerChunk];
        noiseShader.SetBuffer(0, "weights", weightsBuffer);
        
        noiseShader.SetInt("chunkSize",GridMetrics.pointsPerChunk);
        noiseShader.SetFloat("noiseScale",noiseScale);
        noiseShader.SetFloat("amplitude", amplitude);
        noiseShader.SetFloat("frequency", frequency);
        noiseShader.SetInt("octaves", octaves);
        noiseShader.SetFloat("groundPercent", groundPercent);
        
        noiseShader.Dispatch(0, GridMetrics.pointsPerChunk / GridMetrics.numThreads, GridMetrics.pointsPerChunk / GridMetrics.numThreads, GridMetrics.pointsPerChunk / GridMetrics.numThreads);
        weightsBuffer.GetData(heights);
        return heights;
    }
}
