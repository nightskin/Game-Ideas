using UnityEngine;
using Unity.Mathematics;

public class World : MonoBehaviour
{
    //public FastNoiseAsset noise;
    public float isoLevel = 0.5f;
    public float squashFactor = 0.1f;
    public int chunkSize = 64;
    public int baseHeight = 16;
    public string seed = "";

    [SerializeField] GameObject chunkPrefab;
    
    public float GetValue(Vector3 position)
    {
        if(position.y >= baseHeight)
        {
            float squash = VoxelHelper.Remap(position.y,0,chunkSize,0,1) + VoxelHelper.Remap(baseHeight,0,chunkSize,0,1) * squashFactor;
            return 0; //VoxelHelper.Remap(noise.GetNoise(position),-1,1,0,1) - squash;
        }
        else return 1; //VoxelHelper.Remap(noise.GetNoise(position),-1,1,0,1);
    }

    public void CreateRandom()
    {
        for(int x = -1; x <= 1; x++)
        {
            for(int z = -1; z <= 1; z++)
            {
                var chunk = Instantiate(chunkPrefab, new Vector3(x,0,z) * chunkSize, Quaternion.identity, transform);
                chunk.name = new Vector2Int(x,z).ToString();
                chunk.GetComponent<LevelMeshChunk>().GenerateChunk();
            }
        }
    }
}
