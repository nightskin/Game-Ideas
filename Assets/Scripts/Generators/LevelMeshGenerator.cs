using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MonoBehaviourExt
{
    public static void InvokeNextFrame(this MonoBehaviour self, System.Action callback)
    {
        if (self.gameObject.activeInHierarchy && self.enabled)
            self.StartCoroutine(DelayedCall(callback));
    }

    private static IEnumerator DelayedCall(System.Action callback)
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
    public Material chunkMaterial;
    public Transform player;
    public string seed = string.Empty;
    
    [Min(8)] public Vector3Int worldSize = Vector3Int.one * 64;
    [HideInInspector] public Vector3Int maxNumberOfChunks;
    [Min(8)]public Vector3Int chunkSize = new Vector3Int(32,32,32);
    [Min(1)] public Vector3Int startNumberOfChunks = new Vector3Int(2,2,2);

    [Min(0.01f)] public float worldScale = 1;

    public float isoLevel = 0.5f;
    [HideInInspector] public float[] dungeonGrid;

    [Space]
    [Header("DUNGEON SETTINGS")]
    [HideInInspector] public int dungeonIndex = 0;
    public bool useBoxShapedRooms = false;
    [SerializeField] bool useRoundedHallways = false;
    [SerializeField][Min(1)] int numberOfSteps = 200;
    [SerializeField] bool walk3D = false;
    [SerializeField][Min(1)] int numberOfRooms = 2;
    [SerializeField][Min(1)] int ceilngHeight = 2;
    [SerializeField][Min(1)] int minRoomSize = 2;
    [SerializeField][Min(1)] int maxRoomSize = 10;
    [Min(1)] public int hallwaySize = 2;
    


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
            Gizmos.DrawWireCube(transform.position + ((Vector3)worldSize * worldScale / 2 ), (Vector3)worldSize * worldScale);
        }
    }

    void OnValidate()
    {
        if(transform.childCount > 0)
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                transform.localScale = Vector3.one * worldScale;
                Chunk chunk = transform.GetChild(i).GetComponent<Chunk>();
                chunk.renderer.material = chunkMaterial;
                chunk.Generate();
            }
        }
    }

    void Start()
    {
        if(!player) player = GameObject.Find("Player").transform;
        
        if(type == LevelType.DUNGEON)
        {
            for(int x = 0; x < worldSize.x; x++)
            {
                for(int z = 0; z < worldSize.z; z++)
                {
                    Ray ray = new Ray(new Vector3(x,worldSize.y,z) * worldScale, Vector3.down);
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
            Ray ray = new Ray(new Vector3(worldSize.x/2,worldSize.y,worldSize.z/2) * worldScale, Vector3.down);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                player.position = hit.point;
            }
        }
    }
    
    public void Init(bool random)
    {
        if (random)
        {
            seed = System.DateTime.Now.ToString();
        }
        
        maxNumberOfChunks = new Vector3Int(worldSize.x/chunkSize.x, worldSize.y/chunkSize.y, worldSize.z/chunkSize.z);
        transform.localScale = Vector3.one * worldScale;
        Vector3Int halfPoint = maxNumberOfChunks/2;
        
        if(type == LevelType.DUNGEON)
        {
            GenerateDungeonData(useBoxShapedRooms);
            startNumberOfChunks = Vector3Int.one;
            GameObject chunkObject = Instantiate(chunkPrefab,Vector3.zero,Quaternion.identity, transform);
            Chunk chunk = chunkObject.transform.GetComponent<Chunk>();
            chunkSize = worldSize;
            chunk.name = "Dungeon";
            chunk.generator = this;
            if(chunkMaterial) chunk.renderer.material = chunkMaterial;
            chunk.Generate();
        }
        else
        {
            for(int chunkX = -startNumberOfChunks.x/2; chunkX < startNumberOfChunks.x/2; chunkX++)
            {
                for(int chunkY = -startNumberOfChunks.y/2; chunkY < startNumberOfChunks.y/2; chunkY++)
                {
                    for(int chunkZ = -startNumberOfChunks.z/2; chunkZ < startNumberOfChunks.z/2; chunkZ++)
                    {
                        GameObject chunkObject = Instantiate(chunkPrefab,Vector3.zero,Quaternion.identity, transform);
                        Chunk chunk = chunkObject.transform.GetComponent<Chunk>();
                        chunk.index = halfPoint + new Vector3Int(chunkX,chunkY,chunkZ);
                        chunk.name = chunk.index.ToString();
                        chunk.generator = this;
                        if(chunkMaterial) chunk.renderer.material = chunkMaterial;
                        chunk.Generate();
                    }
                }
            }
        }
    }

    public void AddChunk(Vector3Int indexPosition)
    {
        if(indexPosition.x < 0 || indexPosition.x > maxNumberOfChunks.x || indexPosition.z < 0 || indexPosition.z > worldSize.z)
        {
            return;
        }

        GameObject chunkObject = Instantiate(chunkPrefab,Vector3.zero,Quaternion.identity, transform);
        Chunk chunk = chunkObject.transform.GetComponent<Chunk>();
        chunk.index = indexPosition;
        chunk.name = chunk.index.ToString();
        chunk.generator = this;
        if(chunkMaterial) chunk.renderer.material = chunkMaterial;
        chunk.Generate();
    }

    public void AddWater()
    {

    }

    public void GenerateDungeonData(bool boxRooms)
    {
        dungeonGrid = new float[worldSize.x * worldSize.y * worldSize.z];
        for(int i = 0; i < worldSize.x * worldSize.y * worldSize.z; i++)
        {
            dungeonGrid[i] = 1;
        }
        Random.InitState(seed.GetHashCode());
        if(boxRooms)
        {
            //Create Rooms
            List<Vector3Int> rooms = new List<Vector3Int>();
            for (int r = 0; r < numberOfRooms; r++)
            {
                int roomSizeX = Random.Range(minRoomSize, maxRoomSize);
                int roomSizeZ = Random.Range(minRoomSize, maxRoomSize);

                int rx = Random.Range(roomSizeX, worldSize.x - roomSizeX);
                int ry = Random.Range(ceilngHeight, worldSize.y - ceilngHeight);
                int rz = Random.Range(roomSizeZ, worldSize.z - roomSizeZ);
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
                int xi = Random.Range(0, worldSize.x);
                int yi = Random.Range(0, worldSize.y);
                int zi = Random.Range(0, worldSize.z);

                Vector3Int currentIndex = new Vector3Int(xi, yi, zi);
                entrances.Add(currentIndex);
                for (int s = 0; s < numberOfSteps; s++)
                {
                    int x = Random.Range(-1, 2);
                    int y = 0;
                    if (walk3D) y = Random.Range(-1, 2);
                    int z = Random.Range(-1, 2);

                    if (x == -1 && currentIndex.x <= 0) x = 1;
                    if (x == 1 && currentIndex.x >= worldSize.x - 1) x = -1;

                    if (z == -1 && currentIndex.z <= 0) z = 1;
                    if (z == 1 && currentIndex.z >= worldSize.z - 1) z = -1;

                    if (y == -1 && currentIndex.y <= 0) y = 1;
                    if (y == 1 && currentIndex.y >= worldSize.y - 1) y = -1;


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
                    if (cell.x + x >= worldSize.x - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= worldSize.y - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= worldSize.z - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    float maxDistance = Vector3Int.Distance(new Vector3Int(-maxX,-maxY,-maxZ), new Vector3Int(maxX,maxY,maxZ));
                    float distance = Vector3Int.Distance(cell, cell + new Vector3Int(x,y,z));
                    
                    int index = VoxelHelper.Index3DToIndex(new Vector3Int(cell.x + x, cell.y + y, cell.z + z), chunkSize);
                    dungeonGrid[index] -= Util.Remap(distance,0,maxDistance,0,1);
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
                    if (cell.x + x >= worldSize.x - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= worldSize.y - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= worldSize.z - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    int index = VoxelHelper.Index3DToIndex(new Vector3Int(cell.x + x, cell.y + y, cell.z + z), worldSize);
                    if(dungeonGrid[index] < isoLevel)
                    {
                        dungeonGrid[index] = isoLevel - 0.01f;
                    }
                    else
                    {
                         dungeonGrid[index] -= 0.2f;
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
    
}