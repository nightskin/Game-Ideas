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

    public float GetValue(Vector3 position)
    {
        if(position.y == 0) return 0;
        else if(position.y == voxelSize) return 1;

        if(position.y < baseHeight)
        {
            return Mathf.Abs(noise.Evaluate(position * caveScale));
        }

        float value = 0;
        float finalFrequency = 1;
        float finalAmplitude = 1;

        for(int i = 0; i < octaves; i++)
        {
            value += finalAmplitude * noise.Evaluate(new Vector3(position.x, 0, position.z) * terrainScale * finalFrequency);
            finalAmplitude *= 0.5f;
            finalFrequency *= 2;
        }

        value = value - (Remap(position.y,0,worldSize.y,0,1) * squashFactor) + (Remap(baseHeight,0,worldSize.y,0,1) * squashFactor);
        value = Remap(value,0,octaves,0,1);
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
