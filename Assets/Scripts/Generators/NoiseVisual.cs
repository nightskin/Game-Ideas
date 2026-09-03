using UnityEngine;

public class NoiseVisual : MonoBehaviour
{
    //public NoiseGPU noiseGenerator;
    float[] values = new float[LevelMeshChunk.chunkSize * LevelMeshChunk.chunkSize * LevelMeshChunk.chunkSize];
    void Start()
    {
        values = Game.get.noise.GetNoise();
    }

    void OnDrawGizmos()
    {
        if(values == null || values.Length == 0) return;

        for (int x = 0; x < LevelMeshChunk.chunkSize; x++) 
        {
            for (int y = 0; y < LevelMeshChunk.chunkSize; y++) 
            {
                for (int z = 0; z < LevelMeshChunk.chunkSize; z++) 
                {
                    int index = x + LevelMeshChunk.chunkSize * (y + LevelMeshChunk.chunkSize * z);
                    float noiseValue = values[index];
                    Gizmos.color = Color.Lerp(Color.black, Color.white, noiseValue);
                    Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one * .2f);
                }
            }
        } 
    }
}
