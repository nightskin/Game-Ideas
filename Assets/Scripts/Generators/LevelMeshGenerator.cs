using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelMeshGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public bool placePlayerAutomatically;
    public Transform player;
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] string seed = string.Empty;
    [Min(10)] public int gridSize = 100;
    [Min(1)] public int numberOfChunks = 1;
    int chunkSize;
    Noise noise;
    

    public enum LevelType
    {
        DUNGEON,
        CAVES,
        TERRAIN,
    }
    public LevelType levelType = LevelType.DUNGEON;
    public enum ArtStyle
    {
        BLOCKY,
        CHUNKY,
        SMOOTH,
    }
    public ArtStyle style = ArtStyle.CHUNKY;
    [HideInInspector] public float isoLevel = 0.5f;
    [Min(1)] public float voxelSize = 3;
    public float[,,] dungeonGrid = null;

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
            Gizmos.DrawWireCube(transform.position + (Vector3.one * gridSize / 2 * voxelSize), Vector3.one * gridSize * voxelSize);
        }
    }

    public void Create()
    {
        if (seed == string.Empty) seed = System.DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());
        noise = new Noise(seed.GetHashCode());
        dungeonGrid = new float[gridSize, gridSize, gridSize];

        if(levelType == LevelType.DUNGEON) GenerateDungeonData(useBoxShapedRooms);

        chunkSize = gridSize/numberOfChunks;

        
        for(int chunkX = 0; chunkX < numberOfChunks; chunkX++)
        {
            for(int chunkY = 0; chunkY < numberOfChunks; chunkY++)
            {
                for(int chunkZ = 0; chunkZ < numberOfChunks; chunkZ++)
                {
                    Vector3 chunkPosition = new Vector3(chunkX * chunkSize, chunkY * chunkSize, chunkZ * chunkSize) * voxelSize;
                    GameObject chunkObject = Instantiate(chunkPrefab,chunkPosition,Quaternion.identity, transform);
                    chunkObject.name = new Vector3Int(chunkX,chunkY,chunkZ).ToString();
                    LevelMeshChunk chunk = chunkObject.transform.GetComponent<LevelMeshChunk>();
                    chunk.index = new Vector3Int(chunkX,chunkY,chunkZ);
                    chunk.name = chunk.index.ToString();
                    chunk.dungeon = this;
                    chunk.chunkSize = chunkSize;

                    chunk.GenerateChunk();
                }
            }
        }
    }

    public void AddWater()
    {
        
    }

    void Start()
    {
        if(placePlayerAutomatically)
        {
            if(levelType == LevelType.DUNGEON)
            {
                for(int i = 0; i < transform.childCount; i++)
                {
                    LevelMeshChunk chunk = transform.GetChild(i).GetComponent<LevelMeshChunk>();
                    player.transform.position = chunk.transform.position + new Vector3(chunk.chunkSize/2,chunk.chunkSize,chunk.chunkSize/2);
                    if(Physics.Raycast(player.transform.position, Vector3.down,out RaycastHit hit))
                    {
                        player.transform.position = hit.point;
                        break;
                    }
                }
            }
            else if(levelType == LevelType.TERRAIN || levelType == LevelType.CAVES)
            {
                player.transform.position = transform.position + (new Vector3(gridSize/2, gridSize,gridSize/2) * voxelSize);
                if(Physics.Raycast(player.transform.position, Vector3.down,out RaycastHit hit))
                {
                    player.transform.position = hit.point;
                }
            }
        }
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

                int rx = UnityEngine.Random.Range(roomSizeX, gridSize - roomSizeX);
                int ry = UnityEngine.Random.Range(ceilngHeight, gridSize/2 - ceilngHeight);
                int rz = UnityEngine.Random.Range(roomSizeZ, gridSize - roomSizeZ);
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
                int xi = UnityEngine.Random.Range(0, gridSize);
                int yi = UnityEngine.Random.Range(0, gridSize/2);
                int zi = UnityEngine.Random.Range(0, gridSize);

                Vector3Int currentIndex = new Vector3Int(xi, yi, zi);
                entrances.Add(currentIndex);
                for (int s = 0; s < numberOfSteps; s++)
                {
                    int x = UnityEngine.Random.Range(-1, 2);
                    int y = 0;
                    if (walk3D) y = UnityEngine.Random.Range(-1, 2);
                    int z = UnityEngine.Random.Range(-1, 2);

                    if (x == -1 && currentIndex.x <= 0) x = 1;
                    if (x == 1 && currentIndex.x >= gridSize - 1) x = -1;

                    if (z == -1 && currentIndex.z <= 0) z = 1;
                    if (z == 1 && currentIndex.z >= gridSize - 1) z = -1;

                    if (y == -1 && currentIndex.y <= 0) y = 1;
                    if (y == 1 && currentIndex.y >= gridSize - 1) y = -1;


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
                    if (cell.x + x >= gridSize - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= gridSize - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= gridSize - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    float maxDistance = Vector3Int.Distance(new Vector3Int(-maxX,-maxY,-maxZ), new Vector3Int(maxX,maxY,maxZ));
                    float distance = Vector3Int.Distance(cell, cell + new Vector3Int(x,y,z));
                    
                    dungeonGrid[cell.x + x, cell.y + y, cell.z + z] += Util.Remap(distance,0,maxDistance,0,1);
                    dungeonGrid[cell.x + x, cell.y + y, cell.z + z] = Mathf.Clamp01(dungeonGrid[cell.x + x, cell.y + y, cell.z + z]);
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
                    if (cell.x + x >= gridSize - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= gridSize - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= gridSize - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    if(dungeonGrid[cell.x + x, cell.y + y, cell.z + z] < isoLevel)
                    {
                        dungeonGrid[cell.x + x, cell.y + y, cell.z + z] = isoLevel + 0.01f;
                    }
                    else
                    {
                         dungeonGrid[cell.x + x, cell.y + y, cell.z + z] += 0.2f;
                    }
                }
            }
        }
    }

    public float GetDungeonValue(Vector3Int position)
    {
        return dungeonGrid[position.x, position.y, position.z];
    }

    public float GetCaveValue(Vector3 position)
    {
        if(position.x == 0 || position.y == 0 || position.z == 0 || position.x >= gridSize-1 * voxelSize || position.y >= gridSize-1 * voxelSize || position.z >= gridSize-1 * voxelSize)
        {
            return 0;
        }

        float value = 0;
        float frequency = 1;
        float amplitude = 1;
        for(int octave = 0; octave < caveLayers; octave++)
        {
            value += Util.Remap(noise.Evaluate(position * caveNoiseScale * frequency),-1,1,0,1);
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
            value2d += amplitude * Util.Remap(noise.Evaluate(new Vector3(position.x, 0, position.z) * terrainNoiseScale * frequency), -1,1,0,1);
            frequency *= terrainPersistance;
            amplitude /= terrainLacunarity;
        }
        value2d = Util.Remap(value2d,0,terrainLayers,0,1);

        float heightNormalized = Util.Remap(position.y + baseHeight, baseHeight,gridSize - 1 + baseHeight,0,1);
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