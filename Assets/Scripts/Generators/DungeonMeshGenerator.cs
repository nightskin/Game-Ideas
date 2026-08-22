using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMeshGenerator : MonoBehaviour
{
    [Header("Default Parameters")]
    [Tooltip("Player GameObject That will be placed in the level on Runtime")] public Transform player;
    public GameObject chunkPrefab;
    [Tooltip("Determines max size of the level")] public Vector3Int dungeonSize = new Vector3Int(100,20,100);
    [Tooltip("Determines the size of a chunk in the level")] public Vector3Int chunkSize = new Vector3Int(20,20,20);
    public string seed = string.Empty;
    public enum LevelGenerationAlgorithm
    {
        RANDOM_WALKER,
        TINY_KEEP,
        RANDOM_KEEP,
        NOISE,
        LAYERED_NOISE,
    }
    [SerializeField] LevelGenerationAlgorithm levelGeneration = LevelGenerationAlgorithm.RANDOM_KEEP;
    public enum MeshGenerationAlgorithm
    {
        VOXEL_MESH,
        MARCHING_CUBES,
        MARCHING_CUBES_SMOOTH,
    }
    
    public MeshGenerationAlgorithm meshGeneration = MeshGenerationAlgorithm.MARCHING_CUBES;
    [SerializeField] float incrementLevel = 0.01f;
    [Min(1)] public int hallwaySize = 1;
    [Range(0,1)] public float isoLevel = 0;
    [Min(1)] public int voxelSize = 5;
    float[,,] grid = null;

    [Space]
    [Header("RANDOM_WALKER Parameters")]
    [SerializeField][Min(1)] int numberOfSteps = 200;
    [SerializeField] bool walk3D = false;

    [Space]
    [Header("TINY_KEEP Parameters")]
    [SerializeField][Min(1)] int numberOfRooms = 2;
    [SerializeField][Min(1)] int ceilngHeight = 2;
    [SerializeField][Min(1)] int minRoomSize = 2;
    [SerializeField][Min(1)] int maxRoomSize = 10;
    
    [Space]
    [Header("NOISE_METHOD Parameters")]
    [SerializeField][Min(1)] int octaves = 1;
    [SerializeField][Range(0,1)] float persistance = 1;
    [SerializeField] float lacunarity = 1;
    [SerializeField][Range(0,2)] float baseNoiseScale = 0.1f;


    [Header("Debug")]
    [SerializeField] bool showBounds = false;
    [SerializeField] Color boundColor = Color.rebeccaPurple;
    void OnDrawGizmos()
    {
        if (showBounds)
        {
            Gizmos.color = boundColor;
            Gizmos.DrawWireCube(transform.position + ((Vector3)dungeonSize / 2 * voxelSize), (Vector3)dungeonSize * voxelSize);
        }
    }

    void Awake()
    {
        if (seed == string.Empty) seed = System.DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());
        grid = new float[dungeonSize.x+1, dungeonSize.y+1, dungeonSize.z+1];

        GenerateDungeonData(levelGeneration);
        
        Vector3Int numberOfChunks = new Vector3Int(dungeonSize.x/chunkSize.x, dungeonSize.y/chunkSize.y, dungeonSize.z/chunkSize.z);

        for(int chunkX = 0; chunkX < numberOfChunks.x; chunkX++)
        {
            for(int chunkY = 0; chunkY < numberOfChunks.y; chunkY++)
            {
                for(int chunkZ = 0; chunkZ < numberOfChunks.z; chunkZ++)
                {
                    Vector3 chunkPosition = new Vector3(chunkX * chunkSize.x, chunkY * chunkSize.y, chunkZ * chunkSize.z) * voxelSize;
                    GameObject chunkObject = Instantiate(chunkPrefab,chunkPosition,Quaternion.identity, transform);
                    chunkObject.name = new Vector3Int(chunkX,chunkY,chunkZ).ToString();
                    DungeonMeshChunk chunk = chunkObject.transform.GetComponent<DungeonMeshChunk>();
                    chunk.name = new Vector3Int(chunkX,chunkY,chunkZ).ToString();
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
                    
                    chunk.Generate();
                }
            }
        }
        
    }

    void Start()
    {
        int i = UnityEngine.Random.Range(0,transform.childCount);
        DungeonMeshChunk chunk = transform.GetChild(i).GetComponent<DungeonMeshChunk>();

        while(!chunk.generated)
        {
            i = UnityEngine.Random.Range(0,transform.childCount);
            chunk = transform.GetChild(i).GetComponent<DungeonMeshChunk>();
        }

        chunk.PlacePlayer();
    }

    void GenerateDungeonData(LevelGenerationAlgorithm algorithm)
    {
        if (algorithm == LevelGenerationAlgorithm.RANDOM_WALKER)
        {
            RandomWalker();
        }
        else if (algorithm == LevelGenerationAlgorithm.TINY_KEEP)
        {
            TinyKeep();
        }
        else if (algorithm == LevelGenerationAlgorithm.RANDOM_KEEP)
        {
            RandomKeep();
        }
        else if(algorithm == LevelGenerationAlgorithm.NOISE)
        {
            NoiseMethod(false);
        }
        else if(algorithm == LevelGenerationAlgorithm.LAYERED_NOISE)
        {
            NoiseMethod(true);
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
                    if (cell.x + x >= dungeonSize.x - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= dungeonSize.y - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= dungeonSize.z - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    
                    grid[cell.x + x, cell.y + y, cell.z + z] += incrementLevel;
                    grid[cell.x + x, cell.y + y, cell.z + z] = Mathf.Clamp01(grid[cell.x + x, cell.y + y, cell.z + z]);
                }
            }
        }
    }
    
    void NoiseMethod(bool layered = false)
    {
        Noise noise = new Noise(seed.GetHashCode());
        for(int x = 0; x < dungeonSize.x; x++)
        {
            for(int y = 0; y < dungeonSize.y; y++)
            {
                for(int z = 0; z < dungeonSize.z; z++)
                {
                    if(x == 0 || y == 0 || z == 0 || x == dungeonSize.x-1 || y == dungeonSize.y-1 || z == dungeonSize.z-1)
                    {
                        continue;
                    }

                    if(layered)
                    {
                        float amp = 1;
                        float freq = 1;
                        float value = 0;
                        for(int o = 0; o < octaves; o++)
                        {
                            value += noise.Evaluate(new Vector3(x,y,z) * (baseNoiseScale * freq)) * amp;
                            amp *= persistance;
                            freq *= lacunarity;
                        }
                        grid[x,y,z] = value;
                    }
                    else
                    {
                        grid[x,y,z] = Mathf.Clamp01(noise.Evaluate(new Vector3(x,y,z) * baseNoiseScale));
                    }
                }
            }
        }
    }

    void RandomWalker()
    {
        Vector3Int currentIndex = dungeonSize / 2;

        for (int step = 0; step < numberOfSteps; step++)
        {
            int x = UnityEngine.Random.Range(-1, 2);
            int y = 0;
            if (walk3D) y = UnityEngine.Random.Range(-1, 2);
            int z = UnityEngine.Random.Range(-1, 2);

            currentIndex += new Vector3Int(x, y, z);
            ActivateBox(currentIndex, ceilngHeight, ceilngHeight, ceilngHeight);
        }
    }

    void RandomKeep()
    {
        //Create Rooms
        List<Vector3Int> entrances = new List<Vector3Int>();
        List<Vector3Int> exits = new List<Vector3Int>();
        for (int r = 0; r < numberOfRooms; r++)
        {
            int xi = UnityEngine.Random.Range(0, dungeonSize.x);
            int yi = UnityEngine.Random.Range(0, dungeonSize.y);
            int zi = UnityEngine.Random.Range(0, dungeonSize.z);

            Vector3Int currentIndex = new Vector3Int(xi, yi, zi);
            entrances.Add(currentIndex);
            for (int s = 0; s < numberOfSteps; s++)
            {
                int x = UnityEngine.Random.Range(-1, 2);
                int y = 0;
                if (walk3D) y = UnityEngine.Random.Range(-1, 2);
                int z = UnityEngine.Random.Range(-1, 2);

                if (x == -1 && currentIndex.x <= 0) x = 1;
                if (x == 1 && currentIndex.x >= dungeonSize.x - 1) x = -1;

                if (z == -1 && currentIndex.z <= 0) z = 1;
                if (z == 1 && currentIndex.z >= dungeonSize.z - 1) z = -1;

                if (y == -1 && currentIndex.y <= 0) y = 1;
                if (y == 1 && currentIndex.y >= dungeonSize.y - 1) y = -1;


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

    void TinyKeep()
    {
        //Create Rooms
        List<Vector3Int> pointsOfInterest = new List<Vector3Int>();
        for (int r = 0; r < numberOfRooms; r++)
        {
            int roomSizeX = UnityEngine.Random.Range(minRoomSize, maxRoomSize);
            int roomSizeZ = UnityEngine.Random.Range(minRoomSize, maxRoomSize);

            int rx = UnityEngine.Random.Range(roomSizeX, dungeonSize.x - roomSizeX);
            int ry = UnityEngine.Random.Range(ceilngHeight, dungeonSize.y - ceilngHeight);
            int rz = UnityEngine.Random.Range(roomSizeZ, dungeonSize.z - roomSizeZ);
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
            ActivateBox(currentPos, hallwaySize, hallwaySize, hallwaySize);
        }
    } 
    
}