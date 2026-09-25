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
    public LevelMeshGenerator generator;

    ComputeBuffer weightsBuffer;


    void CreateBuffers()
    {
        weightsBuffer = new ComputeBuffer(generator.chunkSize.x * generator.chunkSize.y * generator.chunkSize.z, sizeof(float));
    }

    void ReleaseBuffers()
    {
        weightsBuffer.Release();
    }

    public float[] GetNoise(Vector3Int index)
    {
        CreateBuffers();
        float[] weights = new float[generator.chunkSize.x* generator.chunkSize.y* generator.chunkSize.z];
        noiseShader.SetBuffer(0, "weights", weightsBuffer);
        noiseShader.SetInt("fractalType",(int)generator.fractalType);
        noiseShader.SetInt("noiseType",(int)generator.noiseType);
        noiseShader.SetInt("seed",generator.seed.GetHashCode());
        int[] csize = {generator.chunkSize.x, generator.chunkSize.y, generator.chunkSize.z};
        noiseShader.SetInts("chunkSize",csize);
        int[] wsize = {generator.worldSize.x, generator.worldSize.y, generator.worldSize.z};
        noiseShader.SetInts("worldSize", wsize);
        noiseShader.SetFloat("noiseScale",generator.noiseScale);
        noiseShader.SetFloat("amplitude", generator.amplitude);
        noiseShader.SetFloat("frequency", generator.frequency);
        noiseShader.SetInt("octaves", generator.octaves);
        noiseShader.SetFloat("groundPercent", generator.groundPercent);
        int[] chunkIndex = {index.x, index.y, index.z};
        noiseShader.SetInts("chunkIndex", chunkIndex);
        noiseShader.Dispatch(0, generator.chunkSize.x / Chunk.numThreads, generator.chunkSize.y / Chunk.numThreads, generator.chunkSize.z / Chunk.numThreads);
        weightsBuffer.GetData(weights);
        ReleaseBuffers();
        return weights;
    }
}
