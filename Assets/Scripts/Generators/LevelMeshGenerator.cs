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
    [Min(10)] public int totalSize = 100;
    public bool splitWorldIntoChunks;
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
    public float[,,] grid = null;

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
            Gizmos.DrawWireCube(transform.position + (Vector3.one * totalSize / 2 * voxelSize), Vector3.one * totalSize * voxelSize);
        }
    }

    public void Create()
    {
        if (seed == string.Empty) seed = System.DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());
        noise = new Noise(seed.GetHashCode());
        grid = new float[totalSize, totalSize, totalSize];

        if(levelType == LevelType.DUNGEON) GenerateDungeonData(levelType);
        if(!splitWorldIntoChunks)
        {
            numberOfChunks = 1;
            chunkSize = totalSize;
        }
        else
        {
            chunkSize = totalSize/numberOfChunks;
        }

        
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
                    chunk.map = new float[(int)Mathf.Pow(chunkSize+1,3)];

                    //if(levelType == LevelType.DUNGEON)
                    //{
                    //    for(int x = 0; x < chunkSize; x++)
                    //    {
                    //        for(int y = 0; y < chunkSize; y++)
                    //        {
                    //            for(int z = 0; z < chunkSize; z++)
                    //            {
                    //                Vector3Int gridOffset = new Vector3Int(chunkX * chunkSize, chunkY * chunkSize, chunkZ * chunkSize);
                    //                int i = VoxelHelper.Index3dToIndex(new Vector3Int(x,y,z), chunk.chunkSize);
                    //                chunk.map[i] = grid[x + gridOffset.x, y + gridOffset.y, z + gridOffset.z];
                    //            }
                    //        }
                    //    }
                    //}

                    
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
        if(transform.childCount > 0 && placePlayerAutomatically)
        {
            if(levelType == LevelType.DUNGEON || levelType == LevelType.CAVES)
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
            else if(levelType == LevelType.TERRAIN)
            {
                player.transform.position = transform.position + (new Vector3(totalSize/2, totalSize,totalSize/2) * voxelSize);
                if(Physics.Raycast(player.transform.position, Vector3.down,out RaycastHit hit))
                {
                    player.transform.position = hit.point;
                }
            }
        }
    }
    
    void GenerateDungeonData(LevelType algorithm)
    {
        if (algorithm == LevelType.DUNGEON)
        {
            Dungeon(useBoxShapedRooms);
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
                    if (cell.x + x >= totalSize - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= totalSize - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= totalSize - 1 || cell.z + z <= 0)
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
                    if (cell.x + x >= totalSize - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= totalSize - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= totalSize - 1 || cell.z + z <= 0)
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

    public float GetDungeonValue(Vector3Int position)
    {
        return grid[position.x, position.y, position.z];
    }

    public float GetCaveValue(Vector3 position)
    {
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

        float heightNormalized = Util.Remap(position.y + baseHeight, baseHeight,totalSize - 1 + baseHeight,0,1);
        return value2d + heightNormalized;
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

                int rx = UnityEngine.Random.Range(roomSizeX, totalSize - roomSizeX);
                int ry = UnityEngine.Random.Range(ceilngHeight, totalSize - ceilngHeight);
                int rz = UnityEngine.Random.Range(roomSizeZ, totalSize - roomSizeZ);
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
                int xi = UnityEngine.Random.Range(0, totalSize);
                int yi = UnityEngine.Random.Range(0, totalSize);
                int zi = UnityEngine.Random.Range(0, totalSize);

                Vector3Int currentIndex = new Vector3Int(xi, yi, zi);
                entrances.Add(currentIndex);
                for (int s = 0; s < numberOfSteps; s++)
                {
                    int x = UnityEngine.Random.Range(-1, 2);
                    int y = 0;
                    if (walk3D) y = UnityEngine.Random.Range(-1, 2);
                    int z = UnityEngine.Random.Range(-1, 2);

                    if (x == -1 && currentIndex.x <= 0) x = 1;
                    if (x == 1 && currentIndex.x >= totalSize - 1) x = -1;

                    if (z == -1 && currentIndex.z <= 0) z = 1;
                    if (z == 1 && currentIndex.z >= totalSize - 1) z = -1;

                    if (y == -1 && currentIndex.y <= 0) y = 1;
                    if (y == 1 && currentIndex.y >= totalSize - 1) y = -1;


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