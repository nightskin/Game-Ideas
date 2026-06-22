using UnityEngine;

public class World : MonoBehaviour
{
    SimplexNoise noise;    
    public static World get;
    [SerializeField] GameObject chunkPrefab;
    public Vector2Int worldSize = Vector2Int.one; 
    public Vector3Int chunkSize = new Vector3Int(16,64,16);
    [Min(1)] public int voxelSize = 1;


    public Gradient underGroundColors;
    public Gradient terrainColors;
    public string seed;
    [Range(1,10)]public float persistance = 1;
    [Range(0,1)]public float lacunarity = 1;
    [Range(0,2)]public float terrainScale = 0.1f;
    [Range(0,2)]public float caveScale = 0.15f;
    public int baseHeight = 32;
    [Range(0,1)] public float squashFactor = 0.25f;
    [Range(0,1)] public float isoLevel = 0;
    public int octaves = 3;


    float Remap(float value, float oldMin, float OldMax, float newMin, float newMax)
    {
        float oldRange = OldMax - oldMin;
        float newRange = newMax - newMin;
        return (((value - oldMin) * newRange) / oldRange) + newMin;
    }

    public float GetValue(Vector3 position, Vector3Int index)
    {
        float value = 0;
        float amplitude = 1;
        float frequency = 1;
        for(int i = 0; i < octaves; i++)
        {
            float x = position.x / terrainScale * frequency;
            float z = position.z / terrainScale * frequency;
            value += noise.Evaluate(new Vector3(x,0,z));
            amplitude *= persistance;
            frequency *= lacunarity;
        }
        return value;
    }

    public Color GetColor(Vector3 position)
    {
        float v = Remap(position.y,0,chunkSize.y,0,1);
        if(position.y < baseHeight) return underGroundColors.Evaluate(v);
        else return terrainColors.Evaluate(v);
    }

    public void CreateRandom()
    {
        if(!chunkPrefab) return;

        get = this;
        if(seed == string.Empty)
        {
            seed = Random.value.ToString();
        }
        noise = new SimplexNoise(seed.GetHashCode());

        for(int x = 0; x < worldSize.x; x++)
        {
            for(int z = 0; z < worldSize.y; z++)
            {
                Vector3 position = new Vector3(x * chunkSize.x, 0, z * chunkSize.z) * voxelSize;
                var chunk = Instantiate(chunkPrefab, position, Quaternion.identity, transform).GetComponent<Chunk>();
                chunk.Generate();
            }
        }
    }
}
