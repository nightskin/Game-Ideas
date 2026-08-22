using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMeshGenerator : MonoBehaviour
{
    [Header("General Settings")]
    [Tooltip("Player GameObject That will be placed in the level on Runtime")] public Transform player;
    public GameObject chunkPrefab;
    [Tooltip("Determines max size of the level")] public Vector3Int totalSize = new Vector3Int(100,20,100);
    [Tooltip("Determines the size of a chunk in the level")] public Vector3Int chunkSize = new Vector3Int(100,20,100);
    public string seed = string.Empty;
    bool generated = false;

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

    [Range(0,1)] public float isoLevel = 0.5f;
    [Min(1)] public int tileScale = 5;
    float[,,] grid = null;

    [Space]
    [Header("RANDOM WALK SETTINGS")]
    [SerializeField][Min(1)] int numberOfSteps = 200;
    [SerializeField] bool walk3D = false;

    [Space]
    [Header("ROOM SETTINGS")]
    [SerializeField][Min(1)] int numberOfRooms = 2;
    [SerializeField][Min(1)] int ceilngHeight = 2;
    [SerializeField][Min(1)] int minRoomSize = 2;
    [SerializeField][Min(1)] int maxRoomSize = 10;
    [Min(1)] public int hallwaySize = 2;
    
    [Space]
    [Header("NOISE SETTINGS")]
    [SerializeField][Min(1)] int octaves = 1;
    [SerializeField][Range(0,1)] float persistance = 1;
    [SerializeField] float lacunarity = 1;
    [SerializeField][Range(0,2)] float baseNoiseScale = 0.1f;


    [Header("DEBUG")]
    [SerializeField] bool showBounds = false;
    [SerializeField] Color boundColor = Color.rebeccaPurple;
    void OnDrawGizmos()
    {
        if (showBounds)
        {
            Gizmos.color = boundColor;
            Gizmos.DrawWireCube(transform.position + ((Vector3)totalSize / 2 * tileScale), (Vector3)totalSize * tileScale);
        }
    }

    void Generate()
    {
        if (seed == string.Empty) seed = System.DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());
        grid = new float[totalSize.x+1, totalSize.y+1, totalSize.z+1];

        GenerateDungeonData(levelGeneration);
        
        Vector3Int numberOfChunks = new Vector3Int(totalSize.x/chunkSize.x, totalSize.y/chunkSize.y, totalSize.z/chunkSize.z);

        for(int chunkX = 0; chunkX < numberOfChunks.x; chunkX++)
        {
            for(int chunkY = 0; chunkY < numberOfChunks.y; chunkY++)
            {
                for(int chunkZ = 0; chunkZ < numberOfChunks.z; chunkZ++)
                {
                    Vector3 chunkPosition = new Vector3(chunkX * chunkSize.x, chunkY * chunkSize.y, chunkZ * chunkSize.z) * tileScale;
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
                    
                    chunk.GenerateChunk();
                }
            }
        }
        
        generated = true;
    }

    void Awake()
    {
        if(!generated)
        {
            Generate();
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

                    
                    grid[cell.x + x, cell.y + y, cell.z + z] += incrementLevel;
                    grid[cell.x + x, cell.y + y, cell.z + z] = Mathf.Clamp01(grid[cell.x + x, cell.y + y, cell.z + z]);
                }
            }
        }
    }
    
    void NoiseMethod(bool layered = false)
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

                    if(layered)
                    {
                        float amp = 1;
                        float freq = 1;
                        float value = 0;
                        for(int o = 0; o < octaves; o++)
                        {
                            value += Util.Remap(noise.Evaluate(new Vector3(x,y,z) * (baseNoiseScale * freq)),-1,1,0,1) * amp;
                            amp *= persistance;
                            freq *= lacunarity;
                        }
                        grid[x,y,z] = value;
                    }
                    else
                    {
                        grid[x,y,z] = Util.Remap(noise.Evaluate(new Vector3(x,y,z) * baseNoiseScale),-1,1,0,1);
                    }
                }
            }
        }
    }

    void RandomWalker()
    {
        Vector3Int currentIndex = totalSize / 2;

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

    void TinyKeep()
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