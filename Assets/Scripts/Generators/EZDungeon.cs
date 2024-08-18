using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

struct EZPoint
{
    public bool inRoom;
    public bool isRamp;

    public EZPoint(bool inRoom, bool isRamp)
    {
        this.inRoom = inRoom;
        this.isRamp = isRamp;
    }

}

struct EZRoom
{
    public Vector3 position;
    public Vector3Int size;

    public Vector3[] exits;

    public EZRoom(Vector3 position, Vector3Int size, float tileSize)
    {
        this.position = position;
        this.size = size;

        exits = new Vector3[4];
        exits[0] = position + new Vector3(-size.x - 1, 0, 0) * tileSize;
        exits[1] = position + new Vector3(size.x + 1, 0, 0) * tileSize;
        exits[2] = position + new Vector3(0, 0, -size.z - 1) * tileSize;
        exits[3] = position + new Vector3(0, 0, size.z + 1) * tileSize;
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
        for (int x = -size.x; x <= size.x; x++)
        {
            for (int z = -size.z; z <= size.z; z++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector3 pos = position + (new Vector3(x, y, z) * tileSize);
                    if (!map.ContainsKey(pos))
                    {
                        map.Add(pos, new EZPoint(true, false));
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

    [SerializeField][Min(2)] int worldSizeInUnits = 30;
    [SerializeField] [Min(2)] int numberOfRooms = 2;
    

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
        if (stairPrefab && wallPrefab &&  floorPrefab) 
        {
            Random.InitState(seed.GetHashCode());
            CreateDungeon();
            AddTiles();
        }
        else
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
            //CreateWallMesh();
            //CreateCeilingMesh();

            floors.GetComponent<NavMeshSurface>().collectObjects = CollectObjects.Children;
            floors.GetComponent<NavMeshSurface>().layerMask = floorMask;
            floors.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (map.Keys.Count > 0)
        {
            foreach(Vector3 pos in map.Keys) 
            {
                Gizmos.DrawWireSphere(pos, 1);
            }
        }
    }

    void CreateDungeon()
    {
        //Create Rooms
        EZRoom[] rooms = new EZRoom[numberOfRooms];
        for(int r = 0; r < numberOfRooms; r++)
        {
            rooms[r] = new EZRoom(RandomPosition(-worldSizeInUnits ,worldSizeInUnits), RandomRoomSize(1, 5), tileSize);
            rooms[r].AddRoomToMap(map, tileSize);
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
        if (!map.ContainsKey(currentPos))
        {
            map.Add(currentPos, new EZPoint(false, false));
        }
        while (currentPos != end) 
        {
            if (currentPos.y < end.y)
            {
                if(currentPos.x < end.x) 
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
                currentPos += direction;
                if (!map.ContainsKey(currentPos))
                {
                    map.Add(currentPos, new EZPoint(false, true));
                }
            }
            else if(currentPos.y > end.y)
            {
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
                currentPos += direction;
                if (!map.ContainsKey(currentPos))
                {
                    map.Add(currentPos, new EZPoint(false, true));
                }
            }
            else
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
                    map.Add(currentPos, new EZPoint(false, false));
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
        floorVerts.Add(new Vector3(0.5f, -0.5f, -0.5f) * tileSize + position);
        floorVerts.Add(new Vector3(0.5f, -0.5f, 0.5f) * tileSize + position);

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
        ceilingVerts.Add(new Vector3(0.5f, 0.5f, -0.5f) * tileSize + position);
        ceilingVerts.Add(new Vector3(0.5f, 0.5f, 0.5f) * tileSize + position);

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
        floorVerts.Add(Quaternion.Euler(angle) * new Vector3(-0.5f, 0, -0.5f) * tileSize + position); //0
        floorVerts.Add(Quaternion.Euler(angle) * new Vector3(-0.5f, 1f, 0.5f) * tileSize + position); //1
        floorVerts.Add(Quaternion.Euler(angle) * new Vector3(0.5f, 0, -0.5f) * tileSize + position); //2
        floorVerts.Add(Quaternion.Euler(angle) * new Vector3(0.5f, 1f, 0.5f) * tileSize + position); //3

        //Slope
        floorTris.Add(0 + floorBuffer);
        floorTris.Add(1 + floorBuffer);
        floorTris.Add(2 + floorBuffer);
        floorTris.Add(1 + floorBuffer);
        floorTris.Add(3 + floorBuffer);
        floorTris.Add(2 + floorBuffer);


        //Ramp UV
        floorUvs.Add(new Vector2(0, 0));
        floorUvs.Add(new Vector2(1, 0));
        floorUvs.Add(new Vector2(0, 1));
        floorUvs.Add(new Vector2(1, 1));


        floorBuffer += 4;
    }
    
    void AddTiles()
    {
        foreach(Vector3 key in map.Keys)
        {
            if (!map.ContainsKey(key + Vector3.down * tileSize))
            {
                Instantiate(floorPrefab, key, Quaternion.identity, transform);
            }
            if (!map.ContainsKey(key + new Vector3(1, -1, 0)))
            {
                if (!map[key].inRoom)
                {
                    Instantiate(stairPrefab, key, Quaternion.identity, transform);
                }
            }

        }
    }

    void CreateFloorMesh()
    {
        //Initilize Mesh
        floorMesh = new Mesh();
        floors.GetComponent<MeshFilter>().mesh = floorMesh; 

        foreach(Vector3 key in map.Keys)
        {
            if (map.ContainsKey(key + new Vector3(1, -1, 0) * tileSize))
            {
                Vector3 pos = key + (new Vector3(0, -1.5f, 0) * tileSize);
                CreateRamp(pos, new Vector3(0, -90, 0));
            }
            else if (map.ContainsKey(key + new Vector3(-1, -1, 0) * tileSize))
            {
                Vector3 pos = key + (new Vector3(0, -1.5f, 0) * tileSize);
                CreateRamp(pos, new Vector3(0, 90, 0));
            }
            else if (map.ContainsKey(key + new Vector3(0, -1, 1) * tileSize))
            {
                Vector3 pos = key + (new Vector3(0, -1.5f, 0) * tileSize);
                CreateRamp(pos, new Vector3(0, 180, 0));
            }
            else if (map.ContainsKey(key + new Vector3(0, -1, -1) * tileSize))
            {
                Vector3 pos = key + (new Vector3(0, -1.5f, 0) * tileSize);
                CreateRamp(pos, new Vector3(0, 0, 0));
            }
            else if (!map.ContainsKey(key + Vector3.down * tileSize))
            {
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
            Vector3 pos = key;
            //Add Walls
            if (!map.ContainsKey(key + Vector3.forward * tileSize))
            {
                CreateWallFront(pos);
            }
            if (!map.ContainsKey(key + Vector3.back * tileSize))
            {
                CreateWallBack(pos);
            }
            if (!map.ContainsKey(key + Vector3.right * tileSize))
            {
                CreateWallRight(pos);
            }
            if (!map.ContainsKey(key + Vector3.left * tileSize))
            {
                CreateWallLeft(pos);
            }
            //if (!RampPresent(key))
            //{
            //
            //}
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
            if (!map.ContainsKey(key + Vector3.up * tileSize))
            {
                Vector3 pos = key;
                CreateCeiling(pos);
            }

            //if (!RampPresent(key) && map[key].inRoom)
            //{
            //
            //}

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


    Vector3 RandomPosition(int min, int max)
    {
        int x = Random.Range(min, max + 1);
        int y = Random.Range(min, max + 1);
        int z = Random.Range(min, max + 1);
        return new Vector3(x, y, z) * tileSize;
    }
    
    Vector3Int RandomRoomSize(int min, int max)
    {
        int i = Random.Range(min, max);
        return new Vector3Int(i, 1, i);
    }
}
