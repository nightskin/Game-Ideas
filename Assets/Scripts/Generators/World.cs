using UnityEngine;

public class World : MonoBehaviour
{
    SimplexNoise noise = null;
    public float noiseScale = 0.01f;
    public float isoLevel = 0.5f;
    public int chunkSize = 64;
    public string seed = "";

    [SerializeField] GameObject chunkPrefab;
    
    public float GetValue(Vector3 position)
    {
        if(noise == null) noise = new SimplexNoise(seed.GetHashCode());
        return  Mathf.Clamp01(noise.Evaluate(position * noiseScale));
    }

    public void CreateRandom()
    {
        noise = new SimplexNoise(seed.GetHashCode());

        for(int x = -1; x <= 1; x++)
        {
            for(int z = -1; z <= 1; z++)
            {
                var chunk = Instantiate(chunkPrefab, new Vector3(x,0,z) * chunkSize, Quaternion.identity, transform);
                chunk.name = new Vector2Int(x,z).ToString();
                chunk.GetComponent<Chunk>().Generate();
            }
        }
    }
}
