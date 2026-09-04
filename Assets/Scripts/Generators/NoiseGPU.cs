using UnityEngine;

public class NoiseGPU : MonoBehaviour
{
    public ComputeShader noiseShader;
    [HideInInspector ] public int chunkSize;
    [HideInInspector] public float noiseScale = 1f;
    [HideInInspector] public float amplitude = 5f;
    [HideInInspector] public float frequency = 0.005f;
    [HideInInspector] public int octaves = 8;
    [HideInInspector] public float groundPercent = 0.2f;
    ComputeBuffer weightsBuffer;

    void CreateBuffers()
    {
        weightsBuffer = new ComputeBuffer(chunkSize * chunkSize * chunkSize, sizeof(float));
    }

    void ReleaseBuffers()
    {
        weightsBuffer.Release();
    }

    public float[] GetNoise(Vector3Int index)
    {
        CreateBuffers();

        float[] weights = new float[chunkSize * chunkSize * chunkSize];
        noiseShader.SetBuffer(0, "weights", weightsBuffer);
        
        noiseShader.SetInt("chunkSize",chunkSize);
        noiseShader.SetFloat("noiseScale",noiseScale);
        noiseShader.SetFloat("amplitude", amplitude);
        noiseShader.SetFloat("frequency", frequency);
        noiseShader.SetInt("octaves", octaves);
        noiseShader.SetFloat("groundPercent", groundPercent);
        noiseShader.SetInt("offsetX", index.x);
        noiseShader.SetInt("offsetY", index.y);
        noiseShader.SetInt("offsetZ", index.z);
        noiseShader.Dispatch(0, chunkSize / LevelMeshChunk.numThreads, chunkSize / LevelMeshChunk.numThreads, chunkSize / LevelMeshChunk.numThreads);
        weightsBuffer.GetData(weights);
        ReleaseBuffers();
        return weights;
    }
}
