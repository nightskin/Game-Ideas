using UnityEngine;

public enum NoiseType
{
    NOISE_OPENSIMPLEX2,
    OPENSIMPLEX2S,
    CELLULAR,
    PERLIN ,
    VALUE_CUBIC,
    VALUE,
}
public enum FractalType
{
    NONE,
    FBM,
    RIDGED,
    PING_PONG,
    DOMAIN_WARP_PROGRESSIVE,
    DOMAIN_WARP_INDEPENDENT,
}
public class NoiseGPU : MonoBehaviour
{
    public ComputeShader noiseShader;
    [HideInInspector] public NoiseType noiseType;
    [HideInInspector] public FractalType fractalType;
    [HideInInspector] public string seed;
    [HideInInspector] public bool is3D = false;
    [HideInInspector ] public int chunkSize = 256;
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

    public float[] GetNoise(Vector3 index)
    {
        CreateBuffers();

        float[] weights = new float[chunkSize * chunkSize * chunkSize];
        noiseShader.SetBuffer(0, "weights", weightsBuffer);
        noiseShader.SetInt("fractalType",(int)fractalType);
        noiseShader.SetInt("noiseType",(int)noiseType);
        noiseShader.SetInt("seed",seed.GetHashCode());
        noiseShader.SetBool("is3D", is3D);
        noiseShader.SetInt("chunkSize",chunkSize);
        noiseShader.SetFloat("noiseScale",noiseScale);
        noiseShader.SetFloat("amplitude", amplitude);
        noiseShader.SetFloat("frequency", frequency);
        noiseShader.SetInt("octaves", octaves);
        noiseShader.SetFloat("groundPercent", groundPercent);
        noiseShader.SetFloat("offsetX", index.x);
        noiseShader.SetFloat("offsetY", index.y);
        noiseShader.SetFloat("offsetZ", index.z);
        noiseShader.Dispatch(0, chunkSize / LevelMeshChunk.numThreads, chunkSize / LevelMeshChunk.numThreads, chunkSize / LevelMeshChunk.numThreads);
        weightsBuffer.GetData(weights);
        ReleaseBuffers();
        return weights;
    }
}
