using UnityEngine;

public class World : MonoBehaviour
{
    public string seed;
    public GameObject chunkPrefab;
    public int size = 1;

    public static Noise noise;    

    public static float Evaluate(Vector3 p)
    {
        return noise.Evaluate(p);
    }

    void Start()
    {
        if(!chunkPrefab) return;
        noise = new Noise(seed.GetHashCode());

        float chunkSize = Voxel.size * (Chunk.resolution - 1) / 2;

        for(int x = -size; x <= size; x++)
        {
            for(int z = -size; z <= size; z++)
            {
                Vector3 position = new Vector3(x, 0, z) * chunkSize;
                var c = Instantiate(chunkPrefab, position, Quaternion.identity, transform).GetComponent<Chunk>();
                c.CreateMesh();
            }
        }


    }
}
