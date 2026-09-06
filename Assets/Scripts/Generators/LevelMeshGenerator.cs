using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MonoBehaviourExt
{
    public static void InvokeNextFrame(this MonoBehaviour self, Action callback)
    {
        if (self.gameObject.activeInHierarchy && self.enabled)
            self.StartCoroutine(DelayedCall(callback));
    }

    private static IEnumerator DelayedCall(Action callback)
    {
        yield return null;
        callback?.Invoke();
    }
}

public class LevelMeshGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public LevelType type = LevelType.DUNGEON;
    public LevelStyle style = LevelStyle.CHUNKY;
    public Transform player;
    public Dictionary<Vector3Int, LevelMeshChunk> map = new Dictionary<Vector3Int, LevelMeshChunk>();
    [SerializeField] GameObject chunkPrefab;
    public string seed = string.Empty;
    [Min(10)] public int worldSize = 160;
    [Min(1)] public int chunkSize = 64;
    [HideInInspector] public int numberOfChunks;
    NoiseCPU noiseCPU;

    
    public float isoLevel = 0.5f;
    public static float[] dungeonGrid;
    public static int voxelIndex;

    [Space]
    [Header("DUNGEON SETTINGS")]
    [SerializeField] bool useBoxShapedRooms = false;
    [SerializeField] bool useRoundedHallways = false;
    [SerializeField][Min(1)] int numberOfSteps = 200;
    [SerializeField] bool walk3D = false;
    [SerializeField][Min(1)] int numberOfRooms = 2;
    [SerializeField][Min(1)] int ceilngHeight = 2;
    [SerializeField][Min(1)] int minRoomSize = 2;
    [SerializeField][Min(1)] int maxRoomSize = 10;
    [Min(1)] public int hallwaySize = 2;
    [Min(1)] public int squash = 2;
    


    [Space]
    [Header("NOISE SETTINGS")]
    public NoiseType noiseType;
    public FractalType fractalType;
    public float groundPercent = 0.5f;
    public int octaves = 1;
    public float frequency = 0.5f;
    public float amplitude = 1;
    public float noiseScale = 1;

    [Header("DEBUG")]
    [SerializeField] bool showBounds = false;
    [SerializeField] Color boundColor = Color.rebeccaPurple;


    void OnDrawGizmos()
    {
        if (showBounds)
        {
            Gizmos.color = boundColor;
            Gizmos.DrawWireCube(transform.position + (Vector3.one * worldSize * LevelMeshChunk.chunkScale / 2 ), Vector3.one * worldSize * LevelMeshChunk.chunkScale);
        }
    }

    void Start()
    {
        if(!player) player = GameObject.Find("Player").transform;
        
        for(int x = 0; x < chunkSize; x++)
        {
            for(int y = 0; y < chunkSize; y++)
            {
                for(int z = 0; z < chunkSize; z++)
                {
                    Ray ray = new Ray(new Vector3(x,y,z) * LevelMeshChunk.chunkScale, Vector3.down);
                    if(Physics.Raycast(ray, out RaycastHit hit))
                    {
                        player.position = hit.point;
                        break;
                    }
                }
            }
        }
    }

    void OnValidate()
    {
        if(transform.childCount > 0) this.InvokeNextFrame(() => DestroyKids());
        Generate(false);
    }

    public void Generate(bool random)
    {
        if (random) seed = DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());
        noiseCPU = new NoiseCPU(seed.GetHashCode());

        numberOfChunks = worldSize/chunkSize;
        voxelIndex = 0;
        if(type == LevelType.DUNGEON)
        {
            dungeonGrid = new float[worldSize * worldSize * worldSize];
            GenerateDungeonData(useBoxShapedRooms);
        }
        
        for(int chunkX = 0; chunkX < numberOfChunks; chunkX++)
        {
            for(int chunkY = 0; chunkY < numberOfChunks; chunkY++)
            {
                for(int chunkZ = 0; chunkZ < numberOfChunks; chunkZ++)
                {
                    Vector3 chunkPosition = new Vector3(chunkX * chunkSize, chunkY * chunkSize, chunkZ * chunkSize) * LevelMeshChunk.chunkScale;
                    GameObject chunkObject = Instantiate(chunkPrefab,chunkPosition,Quaternion.identity, transform);
                    LevelMeshChunk chunk = chunkObject.transform.GetComponent<LevelMeshChunkGPU>();
                    if(!chunk)
                    {
                        chunk = chunkObject.transform.GetComponent<LevelMeshChunkCPU>();
                    }
                    chunk.chunkSize = chunkSize;
                    chunk.index = new Vector3Int(chunkX,chunkY,chunkZ);
                    chunk.name = chunk.index.ToString();
                    chunk.generator = this;
                    chunk.Generate();
                }
            }
        }
        transform.localScale = Vector3.one * LevelMeshChunk.chunkScale;
    }

    public void AddWater()
    {

    }

    void GenerateDungeonData(bool boxRooms)
    {
        if(boxRooms)
        {
            //Create Rooms
            List<Vector3Int> rooms = new List<Vector3Int>();
            for (int r = 0; r < numberOfRooms; r++)
            {
                int roomSizeX = UnityEngine.Random.Range(minRoomSize, maxRoomSize);
                int roomSizeZ = UnityEngine.Random.Range(minRoomSize, maxRoomSize);

                int rx = UnityEngine.Random.Range(roomSizeX, worldSize - roomSizeX);
                int ry = UnityEngine.Random.Range(ceilngHeight, worldSize/squash - ceilngHeight);
                int rz = UnityEngine.Random.Range(roomSizeZ, worldSize - roomSizeZ);
                Vector3Int roomPosition = new Vector3Int(rx, ry, rz);
                ActivateBox(roomPosition, roomSizeX, ceilngHeight, roomSizeZ);
                rooms.Add(roomPosition);
            }

            //Create Hallways
            for (int r = 0; r < numberOfRooms - 1; r++)
            {
                Vector3Int start = rooms[r];
                Vector3Int end = rooms[r + 1];
                GenerateHallway(start, end);
            }
        }
        else
        {
            //Create Rooms
            List<Vector3Int> entrances = new List<Vector3Int>();
            List<Vector3Int> exits = new List<Vector3Int>();
            for (int r = 0; r < numberOfRooms; r++)
            {
                int xi = UnityEngine.Random.Range(0, worldSize);
                int yi = UnityEngine.Random.Range(0, worldSize/squash);
                int zi = UnityEngine.Random.Range(0, worldSize);

                Vector3Int currentIndex = new Vector3Int(xi, yi, zi);
                entrances.Add(currentIndex);
                for (int s = 0; s < numberOfSteps; s++)
                {
                    int x = UnityEngine.Random.Range(-1, 2);
                    int y = 0;
                    if (walk3D) y = UnityEngine.Random.Range(-1, 2);
                    int z = UnityEngine.Random.Range(-1, 2);

                    if (x == -1 && currentIndex.x <= 0) x = 1;
                    if (x == 1 && currentIndex.x >= worldSize - 1) x = -1;

                    if (z == -1 && currentIndex.z <= 0) z = 1;
                    if (z == 1 && currentIndex.z >= worldSize - 1) z = -1;

                    if (y == -1 && currentIndex.y <= 0) y = 1;
                    if (y == 1 && currentIndex.y >= worldSize - 1) y = -1;


                    currentIndex += new Vector3Int(x, y, z);
                    ActivateBox(currentIndex, ceilngHeight, ceilngHeight, ceilngHeight);

                }
                exits.Add(currentIndex);
            }

            //Create Hallways
            for (int r = 0; r < numberOfRooms - 1; r++)
            {
                Vector3Int start = entrances[r];
                Vector3Int end = exits[r + 1];
                GenerateHallway(start, end);
            }
        }
    }

    void ActivateSphere(Vector3Int cell, int maxX = 1, int maxY = 1, int maxZ = 1)
    {
        if (dungeonGrid == null) return;
        if (maxX < 1 || maxY < 1 || maxZ < 1) return;

        for (int x = -maxX; x <= maxX; x++)
        {
            for (int y = -maxY; y <= maxY; y++)
            {
                for (int z = -maxZ; z <= maxZ; z++)
                {
                    if (cell.x + x >= worldSize - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= worldSize - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= worldSize - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    float maxDistance = Vector3Int.Distance(new Vector3Int(-maxX,-maxY,-maxZ), new Vector3Int(maxX,maxY,maxZ));
                    float distance = Vector3Int.Distance(cell, cell + new Vector3Int(x,y,z));
                    
                    int index = VoxelHelper.Index3DToIndex(new Vector3Int(cell.x + x, cell.y + y, cell.z + z), chunkSize);
                    dungeonGrid[index] += Util.Remap(distance,0,maxDistance,0,1);
                    dungeonGrid[index] = Mathf.Clamp01(dungeonGrid[index]);
                }
            }
        }
    }
    
    void ActivateBox(Vector3Int cell, int maxX = 1, int maxY = 1, int maxZ = 1)
    {
        if (dungeonGrid == null) return;
        if (maxX < 1 || maxY < 1 || maxZ < 1) return;

        for (int x = -maxX; x <= maxX; x++)
        {
            for (int y = -maxY; y <= maxY; y++)
            {
                for (int z = -maxZ; z <= maxZ; z++)
                {
                    if (cell.x + x >= worldSize - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= worldSize - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= worldSize - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    int index = VoxelHelper.Index3DToIndex(new Vector3Int(cell.x + x, cell.y + y, cell.z + z), chunkSize);
                    if(dungeonGrid[index] < isoLevel)
                    {
                        dungeonGrid[index] = isoLevel + 0.01f;
                    }
                    else
                    {
                         dungeonGrid[index] += 0.2f;
                    }
                }
            }
        }
    }

    public float GetDungeonValue(Vector3Int position)
    {
        int index = VoxelHelper.Index3DToIndex(position, chunkSize);
        return dungeonGrid[index];
    }

    public float GetCaveValue(Vector3 position)
    {
        if(position.x == 0 || position.y == 0 || position.z == 0 || position.x >= worldSize-1 || position.y >= worldSize-1 || position.z >= worldSize-1)
        {
            return 0;
        }

        float value = 0;
        float freq = 1;
        float amp = 1;
        for(int octave = 0; octave < octaves; octave++)
        {
            value += Util.Remap(noiseCPU.Evaluate(position * noiseScale * frequency),-1,1,0,1);
            frequency *= freq;
            amplitude /= amp;
        }
        return Util.Remap(value,0,octaves,0,1);
    }
    
    public float GetTerrainValue(Vector3 position)
    {
        float value2d = 0;
        float freq = 1;
        float amp = 1;
        for(int i = 0; i < octaves; i++)
        {
            value2d += amp * Util.Remap(noiseCPU.Evaluate(new Vector3(position.x, 0, position.z) * noiseScale * freq), -1,1,0,1);
            freq *= frequency;
            amp /= amplitude;
        }
        value2d = Util.Remap(value2d,0,octaves,0,1);

        float heightNormalized = Util.Remap(position.y + groundPercent, groundPercent,worldSize - 1 + groundPercent,0,1);
        return value2d + heightNormalized;
    }
    
    void GenerateHallway(Vector3Int start, Vector3Int end)
    {
        Vector3Int currentPos = start;
        while (currentPos != end)
        {
            Vector3Int[] possibleDirections =
            {
                Vector3Int.left,
                Vector3Int.right,
                Vector3Int.forward,
                Vector3Int.back,
                new Vector3Int(-1, 0, 1),
                new Vector3Int(1,0,1),
                new Vector3Int(-1,0,-1),
                new Vector3Int(1,0,-1),
                new Vector3Int(-1,-1,0),
                new Vector3Int(1,-1,0),
                new Vector3Int(0,-1,1),
                new Vector3Int(0,-1,-1),
                new Vector3Int(1,1,0),
                new Vector3Int(-1,1,0),
                new Vector3Int(0,1,1),
                new Vector3Int(0,1,-1),
                
            };
            Vector3Int chosenDirection = possibleDirections[0];
            foreach (Vector3Int possibleDirection in possibleDirections)
            {
                if (Vector3Int.Distance(currentPos + chosenDirection, end) > Vector3Int.Distance(currentPos + possibleDirection, end))
                {
                    chosenDirection = possibleDirection;
                }
            }

            currentPos += chosenDirection;
            if(useRoundedHallways) ActivateSphere(currentPos,hallwaySize,ceilngHeight,hallwaySize);
            else ActivateBox(currentPos, hallwaySize, ceilngHeight, hallwaySize);
        }
    } 
    
    public void DestroyKids()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
           DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}