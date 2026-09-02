using UnityEngine;

public class NoiseVisual : MonoBehaviour
{
    public NoiseGPU noiseGenerator;
    float[] values = null;
    void Start()
    {
        values = new float[GridMetrics.pointsPerChunk * GridMetrics.pointsPerChunk * GridMetrics.pointsPerChunk];
        values = noiseGenerator.GetNoise();
    }

    void OnDrawGizmos()
    {
        if(values == null || values.Length == 0) return;

        for (int x = 0; x < GridMetrics.pointsPerChunk; x++) 
        {
            for (int y = 0; y < GridMetrics.pointsPerChunk; y++) 
            {
                for (int z = 0; z < GridMetrics.pointsPerChunk; z++) 
                {
                    int index = x + GridMetrics.pointsPerChunk * (y + GridMetrics.pointsPerChunk * z);
                    float noiseValue = values[index];
                    Gizmos.color = Color.Lerp(Color.black, Color.white, noiseValue);
                    Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one * .2f);
                }
            }
        } 
    }
}
