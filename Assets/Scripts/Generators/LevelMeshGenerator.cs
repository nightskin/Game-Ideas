using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelMeshGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public Transform player;
    public Dictionary<Vector3Int, LevelMeshChunkCPU> map = new Dictionary<Vector3Int, LevelMeshChunkCPU>();
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] string seed = string.Empty;
    [Min(10)] public int worldSize = 100;
    [Min(1)] public int numberOfChunks = 1;
    NoiseCPU noiseCPU;
    
    [HideInInspector] public float isoLevel = 0.5f;
    float[,,] dungeon;

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
    
    [Space]
    [Header("CAVE SETTINGS")]
    [SerializeField][Min(1)] int caveLayers = 1;
    [SerializeField][Range(0,1)] float caveNoiseScale = 0.1f;
    [SerializeField] float cavePersistance = 0.5f;
    [SerializeField] float caveLacunarity = 1;

    [Space]
    [Header("TERRAIN SETTINGS")]
    [SerializeField] float waterLevel = 70;
    [SerializeField][Min(0)] int baseHeight = 2; 
    [SerializeField][Min(1)] int terrainLayers = 1;
    [SerializeField] float terrainPersistance = 0.5f;
    [SerializeField] float terrainLacunarity = 1;
    [SerializeField][Range(0,1)] float terrainNoiseScale = 0.1f;

    [Header("DEBUG")]
    [SerializeField] bool showBounds = false;
    [SerializeField] Color boundColor = Color.rebeccaPurple;


    void OnDrawGizmos()
    {
        if (showBounds)
        {
            Gizmos.color = boundColor;
            Gizmos.DrawWireCube(transform.position + (Vector3.one * worldSize / 2 ), Vector3.one * worldSize);
        }
    }

    void Start()
    {
        if(!player) player = GameObject.Find("Player").transform;
        for(int i = 0; i < transform.childCount; i++)
        {
            LevelMeshChunkCPU chunk = transform.GetChild(i).GetComponent<LevelMeshChunkCPU>();
            if(!map.ContainsKey(chunk.index))map.Add(chunk.index,chunk);
            Vector3 checkPosition = chunk.transform.position + new Vector3(LevelMeshChunk.chunkSize/2,LevelMeshChunk.chunkSize,LevelMeshChunk.chunkSize/2) * LevelMeshChunk.chunkScale;
            if(Physics.Raycast(checkPosition, Vector3.down,out RaycastHit hit))
            {
                player.transform.position = hit.point;
            }
        }
    }

    public void Generate()
    {
        if (seed == string.Empty) seed = System.DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());
        noiseCPU = new NoiseCPU(seed.GetHashCode());
        dungeon = new float[worldSize, worldSize, worldSize];

        GenerateDungeonData(useBoxShapedRooms);

        LevelMeshChunk.chunkSize = worldSize/numberOfChunks;
        
        for(int chunkX = 0; chunkX < numberOfChunks; chunkX++)
        {
            for(int chunkY = 0; chunkY < numberOfChunks; chunkY++)
            {
                for(int chunkZ = 0; chunkZ < numberOfChunks; chunkZ++)
                {
                    Vector3 chunkPosition = new Vector3(chunkX * LevelMeshChunk.chunkSize, chunkY * LevelMeshChunk.chunkSize, chunkZ * LevelMeshChunk.chunkSize) * LevelMeshChunk.chunkScale;
                    GameObject chunkObject = Instantiate(chunkPrefab,chunkPosition,Quaternion.identity, transform);
                    chunkObject.transform.localScale = Vector3.one * LevelMeshChunk.chunkScale;
                    chunkObject.name = new Vector3Int(chunkX,chunkY,chunkZ).ToString();
                    LevelMeshChunk chunk = chunkObject.transform.GetComponent<LevelMeshChunkCPU>();
                    if(!chunk)
                    {
                        chunk = chunkObject.transform.GetComponent<LevelMeshChunkGPU>();
                    }
                    chunk.index = new Vector3Int(chunkX,chunkY,chunkZ);
                    chunk.name = chunk.index.ToString();
                    chunk.generator = this;
                    chunk.Generate();
                }
            }
        }
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
                int ry = UnityEngine.Random.Range(ceilngHeight, worldSize/2 - ceilngHeight);
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
                int yi = UnityEngine.Random.Range(0, worldSize/2);
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
        if (dungeon == null) return;
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
                    
                    dungeon[cell.x + x, cell.y + y, cell.z + z] += Util.Remap(distance,0,maxDistance,0,1);
                    dungeon[cell.x + x, cell.y + y, cell.z + z] = Mathf.Clamp01(dungeon[cell.x + x, cell.y + y, cell.z + z]);
                }
            }
        }
    }
    
    void ActivateBox(Vector3Int cell, int maxX = 1, int maxY = 1, int maxZ = 1)
    {
        if (dungeon == null) return;
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

                    if(dungeon[cell.x + x, cell.y + y, cell.z + z] < isoLevel)
                    {
                        dungeon[cell.x + x, cell.y + y, cell.z + z] = isoLevel + 0.01f;
                    }
                    else
                    {
                         dungeon[cell.x + x, cell.y + y, cell.z + z] += 0.2f;
                    }
                }
            }
        }
    }

    public float GetDungeonValue(Vector3Int position)
    {
        return dungeon[position.x, position.y, position.z];
    }

    public float GetCaveValue(Vector3 position)
    {
        if(position.x == 0 || position.y == 0 || position.z == 0 || position.x >= worldSize-1 || position.y >= worldSize-1 || position.z >= worldSize-1)
        {
            return 0;
        }

        float value = 0;
        float frequency = 1;
        float amplitude = 1;
        for(int octave = 0; octave < caveLayers; octave++)
        {
            value += Util.Remap(noiseCPU.Evaluate(position * caveNoiseScale * frequency),-1,1,0,1);
            frequency *= cavePersistance;
            amplitude /= caveLacunarity;
        }
        return Util.Remap(value,0,caveLayers,0,1);
    }
    
    public float GetTerrainValue(Vector3 position)
    {
        float value2d = 0;
        float frequency = 1;
        float amplitude = 1;
        for(int i = 0; i < terrainLayers; i++)
        {
            value2d += amplitude * Util.Remap(noiseCPU.Evaluate(new Vector3(position.x, 0, position.z) * terrainNoiseScale * frequency), -1,1,0,1);
            frequency *= terrainPersistance;
            amplitude /= terrainLacunarity;
        }
        value2d = Util.Remap(value2d,0,terrainLayers,0,1);

        float heightNormalized = Util.Remap(position.y + baseHeight, baseHeight,worldSize - 1 + baseHeight,0,1);
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
    
}