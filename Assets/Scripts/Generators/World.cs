using UnityEngine;

public class World : MonoBehaviour
{
    Noise noise;    
    public static World get;
    [SerializeField] GameObject chunkPrefab;

    public Vector2Int worldSize = Vector2Int.one; 
    public Vector3Int chunkSize = new Vector3Int(16,256,16);

    public string seed;
    public float noiseScale = 0.1f;
    public int baseHeight = 128;
    public int octaves = 3;


    float Remap(float value, float oldMin, float OldMax, float newMin, float newMax)
    {
        float oldRange = OldMax - oldMin;
        float newRange = newMax - newMin;
        return (((value - oldMin) * newRange) / oldRange) + newMin;
    }

    public float Evaluate(Vector3 position)
    {
        if(position.y == 0 || position.y == chunkSize.y - 1)
        {
            return 0;
        }

        float squash = Remap(position.y, 0, 255, 0,1);
        float value = 0;
        float frequency = 1;
        float amplitude = 1;

        for(int i = 0; i < octaves; i++)
        {
            value += amplitude * noise.Evaluate(position * noiseScale * frequency);
            amplitude *= 0.5f;
            frequency *= 2;
        }

        if(position.y > baseHeight)
        {
            value += squash;
        }
        else if(position.y < baseHeight)
        {
            value -= squash;
        }


        return value;
    }

    void Awake()
    {
        get = this;
        if(seed == string.Empty)
        {
            seed = Random.value.ToString();
        }
        noise = new Noise(seed.GetHashCode());
    }

    void Start()
    {
        if(!chunkPrefab) return;

        for(int x = 0; x < worldSize.x; x++)
        {
            for(int z = 0; z < worldSize.y; z++)
            {
                Vector3 position = new Vector3(x * chunkSize.x, 0, z * chunkSize.z);
                var chunk = Instantiate(chunkPrefab, position, Quaternion.identity, transform).GetComponent<Chunk>();
                chunk.Generate();
            }
        }


    }
}
