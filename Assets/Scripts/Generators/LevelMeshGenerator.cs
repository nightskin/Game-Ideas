using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelMeshGenerator : MonoBehaviour
{
    [Header("General Settings")]
    [Tooltip("Player GameObject That will be placed in the level on Runtime")] public Transform player;
    public GameObject chunkPrefab;
    [Tooltip("Determines max size of the level")] public Vector3Int totalSize = new Vector3Int(100,50,100);
    [Min(1)] public Vector3Int numberOfChunks = new Vector3Int(10,5,10);
    Vector3Int chunkSize;
    public string seed = string.Empty;

    public enum LevelType
    {
        DUNGEON,
        CAVES,
        TERRAIN,
    }
    [SerializeField] LevelType levelType = LevelType.DUNGEON;
    public enum ArtStyle
    {
        BLOCKY,
        CHUNKY,
        SMOOTH,
    }
    
    public ArtStyle style = ArtStyle.CHUNKY;
    [HideInInspector] public float isoLevel = 0.5f;
    [Min(1)] public float voxelSize = 3;
    float[,,] grid = null;

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

    [Space]
    [Header("TERRAIN SETTINGS")]
    [SerializeField][Min(0)] int baseHeight = 2; 
    [SerializeField][Min(1)] int terrainLayers = 1;
    [SerializeField] [Range(1,10)] float terrainPersistance = 0.5f;
    [SerializeField] [Range(0,1)] float terrainLacunarity = 1;
    [SerializeField][Range(0,1)] float terrainNoiseScale = 0.1f;

    [Header("DEBUG")]
    [SerializeField] bool showBounds = false;
    [SerializeField] Color boundColor = Color.rebeccaPurple;
    [HideInInspector] public bool largeChunks = false;
    
    void OnDrawGizmos()
    {
        if (showBounds)
        {
            Gizmos.color = boundColor;
            Gizmos.DrawWireCube(transform.position + ((Vector3)totalSize / 2 * voxelSize), (Vector3)totalSize * voxelSize);
        }
    }

    void Generate()
    {
        if (seed == string.Empty) seed = System.DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());
        grid = new float[totalSize.x+1, totalSize.y+1, totalSize.z+1];

        GenerateDungeonData(levelType);
        
        chunkSize = new Vector3Int(totalSize.x/numberOfChunks.x, totalSize.y/numberOfChunks.y, totalSize.z/numberOfChunks.z);

        for(int chunkX = 0; chunkX < numberOfChunks.x; chunkX++)
        {
            for(int chunkY = 0; chunkY < numberOfChunks.y; chunkY++)
            {
                for(int chunkZ = 0; chunkZ < numberOfChunks.z; chunkZ++)
                {
                    Vector3 chunkPosition = new Vector3(chunkX * chunkSize.x, chunkY * chunkSize.y, chunkZ * chunkSize.z) * voxelSize;
                    GameObject chunkObject = Instantiate(chunkPrefab,chunkPosition,Quaternion.identity, transform);
                    chunkObject.name = new Vector3Int(chunkX,chunkY,chunkZ).ToString();
                    LevelMeshChunk chunk = chunkObject.transform.GetComponent<LevelMeshChunk>();
                    chunk.index = new Vector3Int(chunkX,chunkY,chunkZ);
                    chunk.name = chunk.index.ToString();
                    chunk.dungeon = this;
                    chunk.chunkSize = chunkSize;
                    chunk.grid = new float[chunkSize.x+1, chunkSize.y+1, chunkSize.z+1];

                    

                    for(int x = 0; x < chunkSize.x+1; x++)
                    {
                        for(int y = 0; y < chunkSize.y+1; y++)
                        {
                            for(int z = 0; z < chunkSize.z+1; z++)
                            {
                                Vector3Int gridOffset = new Vector3Int(chunkX * chunkSize.x, chunkY * chunkSize.y , chunkZ * chunkSize.z);
                                chunk.grid[x,y,z] = grid[x + gridOffset.x, y + gridOffset.y, z + gridOffset.z];
                            }
                        }
                    }
                    
                    chunk.GenerateChunk();
                }
            }
        }
    }

    void Awake()
    {
        Generate();
    }

    void Start()
    {
        if(transform.childCount > 0)
        {
            int i = UnityEngine.Random.Range(0,transform.childCount); 
            LevelMeshChunk chunk = transform.GetChild(i).GetComponent<LevelMeshChunk>();
            while(!chunk.PlacePlayer())
            {
                i = UnityEngine.Random.Range(0,transform.childCount);
                chunk = transform.GetChild(i).GetComponent<LevelMeshChunk>();
                chunk.PlacePlayer();
            }

        }
    }
    
    void GenerateDungeonData(LevelType algorithm)
    {
        if (algorithm == LevelType.DUNGEON)
        {
            Dungeon(useBoxShapedRooms);
        }
        else if(algorithm == LevelType.CAVES)
        {
            Caves();
        }
        else if(algorithm == LevelType.TERRAIN)
        {
            Terrain();
        }
    }

    void Terrain()
    {
        Noise noise = new Noise(seed.GetHashCode());
        for(int x = 0; x < totalSize.x; x++)
        {
            for(int z = 0; z < totalSize.z; z++)
            {                
                float value2d = 0;
                float frequency = 1;
                float amplitude = 1;
                for(int i = 0; i < terrainLayers; i++)
                {
                    value2d += amplitude * Util.Remap(noise.Evaluate(new Vector3(x, 0, z) * terrainNoiseScale * frequency), -1,1,0,1);
                    frequency *= terrainPersistance;
                    amplitude *= terrainLacunarity;
                }
                value2d = Util.Remap(value2d,0,terrainLayers,0,1);

                for(int y = 0; y < totalSize.y; y++)
                {
                    float heightNormalized = Util.Remap(y,0,totalSize.y-1, 0,1);
                    grid[x,y,z] = value2d + heightNormalized;
                }
            }
        }
    }

    void ActivateSphere(Vector3Int cell, int maxX = 1, int maxY = 1, int maxZ = 1)
    {
        if (grid == null) return;
        if (maxX < 1 || maxY < 1 || maxZ < 1) return;

        for (int x = -maxX; x <= maxX; x++)
        {
            for (int y = -maxY; y <= maxY; y++)
            {
                for (int z = -maxZ; z <= maxZ; z++)
                {
                    if (cell.x + x >= totalSize.x - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= totalSize.y - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= totalSize.z - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    float maxDistance = Vector3Int.Distance(new Vector3Int(-maxX,-maxY,-maxZ), new Vector3Int(maxX,maxY,maxZ));
                    float distance = Vector3Int.Distance(cell, cell + new Vector3Int(x,y,z));
                    
                    grid[cell.x + x, cell.y + y, cell.z + z] += Util.Remap(distance,0,maxDistance,0,1);
                    grid[cell.x + x, cell.y + y, cell.z + z] = Mathf.Clamp01(grid[cell.x + x, cell.y + y, cell.z + z]);
                }
            }
        }
    }
    
    void ActivateBox(Vector3Int cell, int maxX = 1, int maxY = 1, int maxZ = 1)
    {
        if (grid == null) return;
        if (maxX < 1 || maxY < 1 || maxZ < 1) return;

        for (int x = -maxX; x <= maxX; x++)
        {
            for (int y = -maxY; y <= maxY; y++)
            {
                for (int z = -maxZ; z <= maxZ; z++)
                {
                    if (cell.x + x >= totalSize.x - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= totalSize.y - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= totalSize.z - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    if(grid[cell.x + x, cell.y + y, cell.z + z] < isoLevel)
                    {
                        grid[cell.x + x, cell.y + y, cell.z + z] = isoLevel + 0.01f;
                    }
                    else
                    {
                         grid[cell.x + x, cell.y + y, cell.z + z] += 0.2f;
                    }
                }
            }
        }
    }

    void Caves()
    {
        Noise noise = new Noise(seed.GetHashCode());
        for(int x = 0; x < totalSize.x; x++)
        {
            for(int y = 0; y < totalSize.y; y++)
            {
                for(int z = 0; z < totalSize.z; z++)
                {
                    if(x == 0 || y == 0 || z == 0 || x == totalSize.x-1 || y == totalSize.y-1 || z == totalSize.z-1)
                    {
                        continue;
                    }

                    float value = 0;
                    float frequency = 1;
                    for(int octave = 0; octave < caveLayers; octave++)
                    {
                        value += Util.Remap(noise.Evaluate(new Vector3(x,y,z) * caveNoiseScale * frequency),-1,1,0,1);
                        frequency = frequency * octave;
                    }
                    grid[x,y,z] = Util.Remap(value,0,caveLayers,0,1);
                }
            }
        }
    }

    void Dungeon(bool boxRooms)
    {
        if(boxRooms)
        {
            //Create Rooms
            List<Vector3Int> pointsOfInterest = new List<Vector3Int>();
            for (int r = 0; r < numberOfRooms; r++)
            {
                int roomSizeX = UnityEngine.Random.Range(minRoomSize, maxRoomSize);
                int roomSizeZ = UnityEngine.Random.Range(minRoomSize, maxRoomSize);

                int rx = UnityEngine.Random.Range(roomSizeX, totalSize.x - roomSizeX);
                int ry = UnityEngine.Random.Range(ceilngHeight, totalSize.y - ceilngHeight);
                int rz = UnityEngine.Random.Range(roomSizeZ, totalSize.z - roomSizeZ);
                Vector3Int roomPosition = new Vector3Int(rx, ry, rz);
                ActivateBox(roomPosition, roomSizeX, ceilngHeight, roomSizeZ);
                pointsOfInterest.Add(roomPosition);
            }

            //Create Hallways
            for (int r = 0; r < numberOfRooms - 1; r++)
            {
                Vector3Int start = pointsOfInterest[r];
                Vector3Int end = pointsOfInterest[r + 1];
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
                int xi = UnityEngine.Random.Range(0, totalSize.x);
                int yi = UnityEngine.Random.Range(0, totalSize.y);
                int zi = UnityEngine.Random.Range(0, totalSize.z);

                Vector3Int currentIndex = new Vector3Int(xi, yi, zi);
                entrances.Add(currentIndex);
                for (int s = 0; s < numberOfSteps; s++)
                {
                    int x = UnityEngine.Random.Range(-1, 2);
                    int y = 0;
                    if (walk3D) y = UnityEngine.Random.Range(-1, 2);
                    int z = UnityEngine.Random.Range(-1, 2);

                    if (x == -1 && currentIndex.x <= 0) x = 1;
                    if (x == 1 && currentIndex.x >= totalSize.x - 1) x = -1;

                    if (z == -1 && currentIndex.z <= 0) z = 1;
                    if (z == 1 && currentIndex.z >= totalSize.z - 1) z = -1;

                    if (y == -1 && currentIndex.y <= 0) y = 1;
                    if (y == 1 && currentIndex.y >= totalSize.y - 1) y = -1;


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