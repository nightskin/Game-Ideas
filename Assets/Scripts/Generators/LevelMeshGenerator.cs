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
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] Material chunkMaterial;
    public Transform player;
    public Dictionary<Vector3Int, Chunk> map = new Dictionary<Vector3Int, Chunk>();
    public string seed = string.Empty;
    
    [Min(32)] public int worldSizeInVoxels = 128;
    [HideInInspector] public int worldSizeInChunks;
    [Min(8)]public int voxelsInChunk = 32;
    [Min(1)] public int startNumberOfChunks = 2;

    [Min(0.01f)] public float worldScale = 1;

    public float isoLevel = 0.5f;
    [HideInInspector] public float[] dungeonGrid;

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
    [Range(0,1)] public float groundPercent = 0.5f;
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
            Gizmos.DrawWireCube(transform.position + (Vector3.one * worldSizeInVoxels * worldScale / 2 ), Vector3.one * worldSizeInVoxels * worldScale);
        }
    }

    void OnValidate()
    {
        if(transform.childCount > 0 && voxelsInChunk == worldSizeInVoxels)
        {
            MonoBehaviourExt.InvokeNextFrame(this, DestroyKids);
            Init(false);
        }
    }

    void Start()
    {
        if(!player) player = GameObject.Find("Player").transform;
        
        if(type == LevelType.DUNGEON)
        {
            for(int x = 0; x < worldSizeInVoxels; x++)
            {
                for(int z = 0; z < worldSizeInVoxels; z++)
                {
                    Ray ray = new Ray(new Vector3(x,worldSizeInVoxels,z) * worldScale, Vector3.down);
                    if(Physics.Raycast(ray, out RaycastHit hit))
                    {
                        player.position = hit.point;
                        break;
                    }
                }
            }
        }
        else if(type == LevelType.TERRAIN)
        {
            Ray ray = new Ray(new Vector3(worldSizeInVoxels/2,worldSizeInVoxels,worldSizeInVoxels/2) * worldScale, Vector3.down);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                player.position = hit.point;
            }
        }
    }
    
    public void Init(bool random)
    {
        if (random) seed = DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());

        if(type == LevelType.DUNGEON)
        {
            worldSizeInVoxels = 128;
            dungeonGrid = new float[worldSizeInVoxels * worldSizeInVoxels * worldSizeInVoxels];
            voxelsInChunk = worldSizeInVoxels;
            GenerateDungeonData(useBoxShapedRooms);

            GameObject chunkObject = Instantiate(chunkPrefab,Vector3.zero,Quaternion.identity, transform);
            Chunk chunk = chunkObject.transform.GetComponent<Chunk>();
            chunk.size = voxelsInChunk;
            chunk.index = Vector3Int.zero;
            chunk.name = chunk.index.ToString();
            chunk.generator = this;
            if(chunkMaterial) chunk.renderer.material = chunkMaterial;
            chunk.Generate();
        }
        else
        {
            worldSizeInChunks = worldSizeInVoxels/voxelsInChunk;
            int halfPoint = worldSizeInChunks/2;
            
            for(int chunkX = -startNumberOfChunks/2; chunkX < startNumberOfChunks/2; chunkX++)
            {
                for(int chunkY = -startNumberOfChunks/2; chunkY < startNumberOfChunks/2; chunkY++)
                {
                    for(int chunkZ = -startNumberOfChunks/2; chunkZ < startNumberOfChunks/2; chunkZ++)
                    {
                        GameObject chunkObject = Instantiate(chunkPrefab,Vector3.zero,Quaternion.identity, transform);
                        Chunk chunk = chunkObject.transform.GetComponent<Chunk>();
                        chunk.size = voxelsInChunk;
                        chunk.index = new Vector3Int(halfPoint, 0, halfPoint) + new Vector3Int(chunkX,chunkY,chunkZ);
                        chunk.name = chunk.index.ToString();
                        chunk.generator = this;
                        if(chunkMaterial) chunk.renderer.material = chunkMaterial;
                        chunk.Generate();
                    }
                }
            }
        }
        
        transform.localScale = Vector3.one * worldScale;
    }

    public void AddChunk(Vector3Int indexPosition)
    {
        if(indexPosition.x < 0 || indexPosition.x > worldSizeInChunks || indexPosition.z < 0 || indexPosition.z > worldSizeInVoxels)
        {
            return;
        }

        GameObject chunkObject = Instantiate(chunkPrefab,Vector3.zero,Quaternion.identity, transform);
        Chunk chunk = chunkObject.transform.GetComponent<Chunk>();
        chunk.size = voxelsInChunk;
        chunk.index = indexPosition;
        chunk.name = chunk.index.ToString();
        chunk.generator = this;
        if(chunkMaterial) chunk.renderer.material = chunkMaterial;
        chunk.Generate();
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

                int rx = UnityEngine.Random.Range(roomSizeX, worldSizeInVoxels - roomSizeX);
                int ry = UnityEngine.Random.Range(ceilngHeight, worldSizeInVoxels/squash - ceilngHeight);
                int rz = UnityEngine.Random.Range(roomSizeZ, worldSizeInVoxels - roomSizeZ);
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
                int xi = UnityEngine.Random.Range(0, worldSizeInVoxels);
                int yi = UnityEngine.Random.Range(0, worldSizeInVoxels/squash);
                int zi = UnityEngine.Random.Range(0, worldSizeInVoxels);

                Vector3Int currentIndex = new Vector3Int(xi, yi, zi);
                entrances.Add(currentIndex);
                for (int s = 0; s < numberOfSteps; s++)
                {
                    int x = UnityEngine.Random.Range(-1, 2);
                    int y = 0;
                    if (walk3D) y = UnityEngine.Random.Range(-1, 2);
                    int z = UnityEngine.Random.Range(-1, 2);

                    if (x == -1 && currentIndex.x <= 0) x = 1;
                    if (x == 1 && currentIndex.x >= worldSizeInVoxels - 1) x = -1;

                    if (z == -1 && currentIndex.z <= 0) z = 1;
                    if (z == 1 && currentIndex.z >= worldSizeInVoxels - 1) z = -1;

                    if (y == -1 && currentIndex.y <= 0) y = 1;
                    if (y == 1 && currentIndex.y >= worldSizeInVoxels - 1) y = -1;


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
                    if (cell.x + x >= worldSizeInVoxels - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= worldSizeInVoxels - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= worldSizeInVoxels - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    float maxDistance = Vector3Int.Distance(new Vector3Int(-maxX,-maxY,-maxZ), new Vector3Int(maxX,maxY,maxZ));
                    float distance = Vector3Int.Distance(cell, cell + new Vector3Int(x,y,z));
                    
                    int index = VoxelHelper.Index3DToIndex(new Vector3Int(cell.x + x, cell.y + y, cell.z + z), voxelsInChunk);
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
                    if (cell.x + x >= worldSizeInVoxels - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= worldSizeInVoxels - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= worldSizeInVoxels - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    int index = VoxelHelper.Index3DToIndex(new Vector3Int(cell.x + x, cell.y + y, cell.z + z), voxelsInChunk);
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