using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

class EZPoint
{
    public bool isRamp;

    public EZPoint(bool isRamp)
    {
        this.isRamp = isRamp;
    }
}
struct EZRoom
{
    public Vector3 position;
    public Vector3 size;
    public Vector3Int sizeInTiles;

    public Vector3[] exits;

    public EZRoom(Vector3 position, Vector3Int sizeInTiles, float tileSize)
    {
        this.position = position;
        this.sizeInTiles = sizeInTiles;
        this.size = (Vector3)sizeInTiles * tileSize;

        exits = new Vector3[4];
        exits[0] = position + new Vector3(-sizeInTiles.x - 1, 0, 0) * tileSize;
        exits[1] = position + new Vector3(sizeInTiles.x + 1, 0, 0) * tileSize;
        exits[2] = position + new Vector3(0, 0, -sizeInTiles.z - 1) * tileSize;
        exits[3] = position + new Vector3(0, 0, sizeInTiles.z + 1) * tileSize;
    }

    public Vector3 GetNearestExit(Vector3 toPosition)
    {
        Vector3 nearest = exits[0];
        for (int i = 1; i < exits.Length; i++) 
        {
            if(Vector3.Distance(toPosition, exits[i]) < Vector3.Distance(toPosition, nearest))
            {
                nearest = exits[i];
            }
        }
        return nearest;
    }

    public void AddRoomToMap(Dictionary<Vector3,EZPoint> map, float tileSize)
    {
        for (int x = -sizeInTiles.x; x <= sizeInTiles.x; x++)
        {
            for (int z = -sizeInTiles.z; z <= sizeInTiles.z; z++)
            {
                for (int y = 0; y < sizeInTiles.y; y++)
                {
                    Vector3 pos = position + (new Vector3(x, y, z) * tileSize);
                    if (!map.ContainsKey(pos))
                    {
                        map.Add(pos, new EZPoint(false));
                    }
                }
            }
        }
    }
}

public class EZDungeon : MonoBehaviour
{
    [SerializeField] string seed;
    [SerializeField] float tileSize = 10;

    [SerializeField] [Min(2)] int numberOfRooms = 2;
    [SerializeField] float roomOffset = 100;

    [SerializeField] GameObject stairPrefab;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject floorPrefab;

    [SerializeField] Material floorMaterial;
    [SerializeField] Material wallMaterial;
    [SerializeField] Material ceilingMaterial;

    [SerializeField] LayerMask floorMask;

    Dictionary<Vector3,EZPoint> map = new Dictionary<Vector3, EZPoint>();

    GameObject floors;
    GameObject ceiling;
    GameObject walls;

    Mesh floorMesh;
    Mesh wallMesh;
    Mesh ceilingMesh;

    List<Vector3> floorVerts = new List<Vector3>();
    List<Vector2> floorUvs = new List<Vector2>();
    List<int> floorTris = new List<int>();

    List<Vector3> wallVerts = new List<Vector3>();
    List<Vector2> wallUvs = new List<Vector2>();
    List<int> wallTris = new List<int>();


    List<Vector3> ceilingVerts = new List<Vector3>();
    List<Vector2> ceilingUvs = new List<Vector2>();
    List<int> ceilingTris = new List<int>();

    int floorBuffer = 0;
    int wallBuffer = 0;
    int ceilingBuffer = 0;

    void Start()
    {
        walls = new GameObject();
        walls.name = "Walls";
        walls.AddComponent<MeshFilter>();
        walls.AddComponent<MeshRenderer>();
        walls.GetComponent<MeshRenderer>().material = wallMaterial;
        walls.AddComponent<MeshCollider>();
        walls.isStatic = true;
        walls.transform.parent = transform;

        ceiling = new GameObject();
        ceiling.name = "Ceiling";
        ceiling.AddComponent<MeshFilter>();
        ceiling.AddComponent<MeshRenderer>();
        ceiling.GetComponent<MeshRenderer>().material = ceilingMaterial;
        ceiling.AddComponent<MeshCollider>();
        ceiling.isStatic = true;
        ceiling.transform.parent = transform;

        floors = new GameObject();
        floors.name = "Floors";
        floors.layer = 6;
        floors.AddComponent<MeshFilter>();
        floors.AddComponent<MeshRenderer>();
        floors.GetComponent<MeshRenderer>().material = floorMaterial;
        floors.AddComponent<MeshCollider>();
        floors.AddComponent<NavMeshSurface>();
        floors.isStatic = true;
        floors.transform.parent = transform;

        Random.InitState(seed.GetHashCode());
        CreateDungeon();

        CreateFloorMesh();
        CreateWallMesh();
        CreateCeilingMesh();

        floors.GetComponent<NavMeshSurface>().collectObjects = CollectObjects.Children;
        floors.GetComponent<NavMeshSurface>().layerMask = floorMask;
        floors.GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    void OnDrawGizmos()
    {

        if (map.Keys.Count > 0)
        {
            foreach(Vector3 key in map.Keys) 
            {
                if (map[key].isRamp)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(key, 1);
                }
                else
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(key, 1);
                }
            }
        }
    }

    void CreateDungeon()
    {
        //Create Rooms
        EZRoom[] rooms = new EZRoom[numberOfRooms];
        for(int r = 0; r < numberOfRooms; r++)
        {
            if(r == 0)
            {
                rooms[r] = new EZRoom(Vector3.zero, RandomRoomSize(2, 5), tileSize);
                rooms[r].AddRoomToMap(map, tileSize);
            }
            else
            {
                Vector3 pos = rooms[r - 1].position + (RandomDirection() * roomOffset);
                rooms[r] = new EZRoom(pos, RandomRoomSize(2, 5), tileSize);
                rooms[r].AddRoomToMap(map, tileSize);
            }
        }

        //Place player in a room
        GameObject.FindGameObjectWithTag("Player").transform.position = rooms[0].position;

        //Create Hallways
        for (int r = 0; r < numberOfRooms; r++)
        {
            if (r + 1 < numberOfRooms)
            {
                CreateHallway(rooms[r], rooms[r + 1]);
            }
        }
    }
    
    void CreateHallway(EZRoom from, EZRoom to)
    {
        Vector3 start = from.GetNearestExit(to.position);
        Vector3 end = to.GetNearestExit(from.position);
        Vector3 currentPos = start;
        Vector3 direction = Vector3.zero;
        Vector3 nextDirection = Vector3.zero;

        if (!map.ContainsKey(start))
        {
            map.Add(start, new EZPoint(true));
        }
        while (currentPos != end) 
        {
            if (currentPos.y < end.y)
            {
                //Calculate Durrent Direction
                if (currentPos.x < end.x) 
                {
                    direction = new Vector3(tileSize, tileSize, 0);
                }
                else if(currentPos.x > end.x)
                {
                    direction = new Vector3(-tileSize, tileSize, 0);
                }
                else if(currentPos.z < end.z)
                {
                    direction = new Vector3(0, tileSize, tileSize);
                }
                else if(currentPos.z > end.z)
                {
                    direction = new Vector3(0, tileSize, -tileSize);
                }

                //Calculate Next Direction
                if ((currentPos + direction).x < end.x)
                {
                    nextDirection = new Vector3(tileSize, tileSize, 0);
                }
                else if ((currentPos + direction).x > end.x)
                {
                    nextDirection = new Vector3(-tileSize, tileSize, 0);
                }
                else if ((currentPos + direction).z < end.z)
                {
                    nextDirection = new Vector3(0, tileSize, tileSize);
                }
                else if ((currentPos + direction).z > end.z)
                {
                    nextDirection = new Vector3(0, tileSize, -tileSize);
                }

                if (direction != nextDirection)
                {
                    direction.y = 0;
                    currentPos += direction;
                    if (!map.ContainsKey(currentPos))
                    {
                        map.Add(currentPos, new EZPoint(false));
                    }
                    

                    currentPos += nextDirection;
                    if (!map.ContainsKey(currentPos))
                    {
                        map.Add(currentPos, new EZPoint(true));
                    }
                }
                else
                {
                    currentPos += direction;
                    if (!map.ContainsKey(currentPos))
                    {
                        map.Add(currentPos, new EZPoint(true));
                    }
                }

            }
            else if(currentPos.y > end.y)
            {
                //Calculate current Direction
                if (currentPos.x < end.x)
                {
                    direction = new Vector3(tileSize, -tileSize, 0);
                }
                else if (currentPos.x > end.x)
                {
                    direction = new Vector3(-tileSize, -tileSize, 0);
                }
                else if (currentPos.z < end.z)
                {
                    direction = new Vector3(0, -tileSize, tileSize);
                }
                else if (currentPos.z > end.z)
                {
                    direction = new Vector3(0, -tileSize, -tileSize);
                }

                //Calculate Next Direction
                if ((currentPos + direction).x < end.x)
                {
                    nextDirection = new Vector3(tileSize, -tileSize, 0);
                }
                else if ((currentPos + direction).x > end.x)
                {
                    nextDirection = new Vector3(-tileSize, -tileSize, 0);
                }
                else if ((currentPos + direction).z < end.z)
                {
                    nextDirection = new Vector3(0, -tileSize, tileSize);
                }
                else if ((currentPos + direction).z > end.z)
                {
                    nextDirection = new Vector3(0, -tileSize, -tileSize);
                }

                if (direction != nextDirection)
                {
                    currentPos += direction;
                    if (!map.ContainsKey(currentPos))
                    {
                        map.Add(currentPos, new EZPoint(false));
                    }

                    nextDirection.y = 0;
                    currentPos += nextDirection;
                    if (!map.ContainsKey(currentPos))
                    {
                        map.Add(currentPos, new EZPoint(true));
                    }
                }
                else
                {
                    currentPos += direction;
                    if (!map.ContainsKey(currentPos))
                    {
                        map.Add(currentPos, new EZPoint(true));
                    }
                }

            }
            else if(currentPos.y == end.y)
            {
                if (currentPos.x < end.x)
                {
                    direction = new Vector3(tileSize, 0, 0);
                }
                else if (currentPos.x > end.x)
                {
                    direction = new Vector3(-tileSize, 0, 0);
                }
                else if (currentPos.z < end.z)
                {
                    direction = new Vector3(0, 0, tileSize);
                }
                else if (currentPos.z > end.z)
                {
                    direction = new Vector3(0, 0, -tileSize);
                }

                currentPos += direction;
                if (!map.ContainsKey(currentPos))
                {
                    map.Add(currentPos, new EZPoint(false));
                }
                else
                {

                }
            }
        }
    }
    
    void CreateWallBack(Vector3 position)
    {
        wallVerts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * tileSize + position);
        wallVerts.Add(new Vector3(0.5f, -0.5f, -0.5f) * tileSize + position);
        wallVerts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * tileSize + position);
        wallVerts.Add(new Vector3(0.5f, 0.5f, -0.5f) * tileSize + position);

        wallTris.Add(1 + wallBuffer);
        wallTris.Add(0 + wallBuffer);
        wallTris.Add(2 + wallBuffer);
        wallTris.Add(3 + wallBuffer);
        wallTris.Add(0 + wallBuffer);
        wallTris.Add(1 + wallBuffer);

        wallUvs.Add(new Vector2(0, 0));
        wallUvs.Add(new Vector2(1, 0));
        wallUvs.Add(new Vector2(0, 1));
        wallUvs.Add(new Vector2(1, 1));

        wallBuffer += 4;
    }

    void CreateWallFront(Vector3 position)
    {
        wallVerts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * tileSize + position);
        wallVerts.Add(new Vector3(0.5f, -0.5f, 0.5f) * tileSize + position);
        wallVerts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * tileSize + position);
        wallVerts.Add(new Vector3(0.5f, 0.5f, 0.5f) * tileSize + position);

        wallTris.Add(1 + wallBuffer);
        wallTris.Add(2 + wallBuffer);
        wallTris.Add(0 + wallBuffer);
        wallTris.Add(0 + wallBuffer);
        wallTris.Add(3 + wallBuffer);
        wallTris.Add(1 + wallBuffer);

        wallUvs.Add(new Vector2(0, 0));
        wallUvs.Add(new Vector2(1, 0));
        wallUvs.Add(new Vector2(0, 1));
        wallUvs.Add(new Vector2(1, 1));

        wallBuffer += 4;
    }

    void CreateWallRight(Vector3 position)
    {
        wallVerts.Add(new Vector3(0.5f, 0.5f, 0.5f) * tileSize + position);
        wallVerts.Add(new Vector3(0.5f, -0.5f, 0.5f) * tileSize + position);
        wallVerts.Add(new Vector3(0.5f, -0.5f, -0.5f) * tileSize + position);
        wallVerts.Add(new Vector3(0.5f, 0.5f, -0.5f) * tileSize + position);

        wallTris.Add(0 + wallBuffer);
        wallTris.Add(3 + wallBuffer);
        wallTris.Add(1 + wallBuffer);
        wallTris.Add(2 + wallBuffer);
        wallTris.Add(1 + wallBuffer);
        wallTris.Add(3 + wallBuffer);

        wallUvs.Add(new Vector2(0, 0));
        wallUvs.Add(new Vector2(1, 0));
        wallUvs.Add(new Vector2(0, 1));
        wallUvs.Add(new Vector2(1, 1));

        wallBuffer += 4;
    }

    void CreateWallLeft(Vector3 position)
    {
        wallVerts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * tileSize + position);
        wallVerts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * tileSize + position);
        wallVerts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * tileSize + position);
        wallVerts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * tileSize + position);

        wallTris.Add(3 + wallBuffer);
        wallTris.Add(0 + wallBuffer);
        wallTris.Add(1 + wallBuffer);
        wallTris.Add(1 + wallBuffer);
        wallTris.Add(2 + wallBuffer);
        wallTris.Add(3 + wallBuffer);

        wallUvs.Add(new Vector2(0, 0));
        wallUvs.Add(new Vector2(1, 0));
        wallUvs.Add(new Vector2(0, 1));
        wallUvs.Add(new Vector2(1, 1));

        wallBuffer += 4;
    }

    void CreateFloor(Vector3 position)
    {
        floorVerts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * tileSize + position);
        floorVerts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * tileSize + position);
        floorVerts.Add(new Vector3(0.5f,  -0.5f, -0.5f) * tileSize + position);
        floorVerts.Add(new Vector3(0.5f,  -0.5f, 0.5f) * tileSize + position);

        floorTris.Add(0 + floorBuffer);
        floorTris.Add(1 + floorBuffer);
        floorTris.Add(2 + floorBuffer);

        floorTris.Add(1 + floorBuffer);
        floorTris.Add(3 + floorBuffer);
        floorTris.Add(2 + floorBuffer);

        floorUvs.Add(new Vector2(0, 0));
        floorUvs.Add(new Vector2(1, 0));
        floorUvs.Add(new Vector2(0, 1));
        floorUvs.Add(new Vector2(1, 1));


        floorBuffer += 4;

    }

    void CreateCeiling(Vector3 position)
    {
        ceilingVerts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * tileSize + position);
        ceilingVerts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * tileSize + position);
        ceilingVerts.Add(new Vector3(0.5f,  0.5f, -0.5f) * tileSize + position);
        ceilingVerts.Add(new Vector3(0.5f,  0.5f, 0.5f) * tileSize + position);

        ceilingTris.Add(1 + ceilingBuffer);
        ceilingTris.Add(0 + ceilingBuffer);
        ceilingTris.Add(2 + ceilingBuffer);
        ceilingTris.Add(3 + ceilingBuffer);
        ceilingTris.Add(1 + ceilingBuffer);
        ceilingTris.Add(2 + ceilingBuffer);

        ceilingUvs.Add(new Vector2(0, 0));
        ceilingUvs.Add(new Vector2(1, 0));
        ceilingUvs.Add(new Vector2(0, 1));
        ceilingUvs.Add(new Vector2(1, 1));


        ceilingBuffer += 4;

    }

    void CreateRamp(Vector3 position, Vector3 angle)
    {
        //Ramp Verts
        floorVerts.Add(Quaternion.Euler(angle) * new Vector3(-0.5f, -0.5f, -0.5f) * tileSize + position); //0
        floorVerts.Add(Quaternion.Euler(angle) * new Vector3(-0.5f, 0.5f, 0.5f) * tileSize + position); //1
        floorVerts.Add(Quaternion.Euler(angle) * new Vector3(0.5f, -0.5f, -0.5f) * tileSize + position); //2
        floorVerts.Add(Quaternion.Euler(angle) * new Vector3(0.5f, 0.5f, 0.5f) * tileSize + position); //3


        //Ramp Verts
        floorVerts.Add(Quaternion.Euler(angle) * new Vector3(-0.5f, 0.5f, -0.5f) * tileSize + position); //4
        floorVerts.Add(Quaternion.Euler(angle) * new Vector3(-0.5f, 1.5f, 0.5f) * tileSize + position); //5
        floorVerts.Add(Quaternion.Euler(angle) * new Vector3(0.5f, 0.5f, -0.5f) * tileSize + position); //6
        floorVerts.Add(Quaternion.Euler(angle) * new Vector3(0.5f, 1.5f, 0.5f) * tileSize + position); //7


        //Floor
        floorTris.Add(0 + floorBuffer);
        floorTris.Add(1 + floorBuffer);
        floorTris.Add(2 + floorBuffer);
        floorTris.Add(1 + floorBuffer);
        floorTris.Add(3 + floorBuffer);
        floorTris.Add(2 + floorBuffer);

        //Ceiling
        floorTris.Add(6 + floorBuffer);
        floorTris.Add(5 + floorBuffer);
        floorTris.Add(4 + floorBuffer);
        floorTris.Add(6 + floorBuffer);
        floorTris.Add(7 + floorBuffer);
        floorTris.Add(5 + floorBuffer);

        //Left Wall
        floorTris.Add(0 + floorBuffer);
        floorTris.Add(4 + floorBuffer);
        floorTris.Add(5 + floorBuffer);
        floorTris.Add(1 + floorBuffer);
        floorTris.Add(0 + floorBuffer);
        floorTris.Add(5 + floorBuffer);

        //Right Wall
        floorTris.Add(2 + floorBuffer);
        floorTris.Add(3 + floorBuffer);
        floorTris.Add(7 + floorBuffer);
        floorTris.Add(6 + floorBuffer);
        floorTris.Add(2 + floorBuffer);
        floorTris.Add(7 + floorBuffer);

        //Ramp UV
        floorUvs.Add(new Vector2(0, 0));
        floorUvs.Add(new Vector2(1, 0));
        floorUvs.Add(new Vector2(0, 1));
        floorUvs.Add(new Vector2(1, 1));

        floorUvs.Add(new Vector2(0, 0));
        floorUvs.Add(new Vector2(1, 0));
        floorUvs.Add(new Vector2(0, 1));
        floorUvs.Add(new Vector2(1, 1));

        floorBuffer += 8;
    }
    
    void CreateFloorMesh()
    {
        //Initilize Mesh
        floorMesh = new Mesh();
        floors.GetComponent<MeshFilter>().mesh = floorMesh; 

        foreach(Vector3 key in map.Keys)
        {
            if (map.ContainsKey(key + new Vector3(-1, -1, 0) * tileSize))
            {
                if (!map[key].isRamp) map[key].isRamp = true;
                Vector3 pos = key + Vector3.down * tileSize;
                CreateRamp(pos, new Vector3(0, 90, 0));
            }
            else if (map.ContainsKey(key + new Vector3(1, -1, 0) * tileSize))
            {
                if (!map[key].isRamp) map[key].isRamp = true;
                Vector3 pos = key + Vector3.down * tileSize;
                CreateRamp(pos, new Vector3(0, -90, 0));
            }
            else if (map.ContainsKey(key + new Vector3(0, -1, 1) * tileSize))
            {
                if (!map[key].isRamp) map[key].isRamp = true;
                Vector3 pos = key + Vector3.down * tileSize;
                CreateRamp(pos, new Vector3(0, 180, 0));
            }
            else if (map.ContainsKey(key + new Vector3(0, -1, -1) * tileSize))
            {
                if (!map[key].isRamp) map[key].isRamp = true;
                Vector3 pos = key + Vector3.down * tileSize;
                CreateRamp(pos, new Vector3(0, 0, 0));
            }
            else if (!map.ContainsKey(key + Vector3.down * tileSize))
            {
                if (map[key].isRamp) map[key].isRamp = false;
                CreateFloor(key);
            }
        }


        //Draw Mesh
        floorMesh.Clear();
        floorMesh.vertices = floorVerts.ToArray();
        floorMesh.triangles = floorTris.ToArray();
        floorMesh.uv = floorUvs.ToArray();
        floorMesh.RecalculateNormals();
        floorMesh.RecalculateTangents();

        floors.GetComponent<MeshCollider>().sharedMesh = floorMesh;
    }

    void CreateWallMesh()
    {
        wallMesh = new Mesh();
        walls.GetComponent<MeshFilter>().mesh = wallMesh;

        //Populate Mesh Values
        foreach(Vector3 key in map.Keys)
        {
            if (!map[key].isRamp)
            {
                if (!map.ContainsKey(key + Vector3.forward * tileSize))
                {
                    if (!map.ContainsKey(key + new Vector3(0, 1, 1) * tileSize))
                    {
                        if (!map.ContainsKey(key + new Vector3(0, -1, 1) * tileSize))
                        {
                            CreateWallFront(key);
                        }
                    }
                }
                if (!map.ContainsKey(key + Vector3.back * tileSize))
                {
                    if(!map.ContainsKey(key + new Vector3(0,1,-1) * tileSize))
                    {
                        if (!map.ContainsKey(key + new Vector3(0, -1, -1) * tileSize))
                        {
                            CreateWallBack(key);
                        }
                    }
                }
                if (!map.ContainsKey(key + Vector3.right * tileSize))
                {
                    if (!map.ContainsKey(key + new Vector3(1, 1, 0) * tileSize))
                    {
                        if (!map.ContainsKey(key + new Vector3(1, -1, 0) * tileSize))
                        {
                            CreateWallRight(key);
                        }
                    }
                }
                if (!map.ContainsKey(key + Vector3.left * tileSize))
                {
                    if (!map.ContainsKey(key + new Vector3(-1, 1, 0) * tileSize))
                    {
                        if (!map.ContainsKey(key + new Vector3(-1, -1, 0) * tileSize))
                        {
                            CreateWallLeft(key);
                        }
                    }

                }
            }
        }

        //Draw Mesh
        wallMesh.Clear();
        wallMesh.vertices = wallVerts.ToArray();
        wallMesh.triangles = wallTris.ToArray();
        wallMesh.uv = wallUvs.ToArray();
        wallMesh.RecalculateNormals();
        wallMesh.RecalculateTangents();

        walls.GetComponent<MeshCollider>().sharedMesh = wallMesh;

    }

    void CreateCeilingMesh()
    {
        ceilingMesh = new Mesh();
        ceiling.GetComponent<MeshFilter>().mesh = ceilingMesh;

        //Populate Mesh Values
        foreach(Vector3 key in map.Keys)
        {
            if(!map[key].isRamp)
            {
                if (!map.ContainsKey(key + Vector3.up * tileSize))
                {
                    CreateCeiling(key);
                }
            }
        }

        //Draw Mesh
        ceilingMesh.Clear();
        ceilingMesh.vertices = ceilingVerts.ToArray();
        ceilingMesh.triangles = ceilingTris.ToArray();
        ceilingMesh.uv = ceilingUvs.ToArray();
        ceilingMesh.RecalculateNormals();
        ceilingMesh.RecalculateTangents();

        ceiling.GetComponent<MeshCollider>().sharedMesh = ceilingMesh;

    }
    
    Vector3 RandomDirection()
    {
        int x = Mathf.RoundToInt(Random.value);
        if (x == 0) x = -1;
        int z = Mathf.RoundToInt(Random.value);
        if(z == 0) z = -1;
        int y = Random.Range(-1, 2);

        return new Vector3(x, y, z);
    }

    Vector3Int RandomRoomSize(int min, int max)
    {
        int i = Random.Range(min, max);
        return new Vector3Int(i, 1, i);
    }
}
