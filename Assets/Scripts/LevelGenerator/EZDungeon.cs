using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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
        exits[0] = position + new Vector3(-size.x, 0, 0) * tileSize;
        exits[1] = position + new Vector3(size.x, 0, 0) * tileSize;
        exits[2] = position + new Vector3(0, 0, -size.z) * tileSize;
        exits[3] = position + new Vector3(0, 0, size.z) * tileSize;
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

    public void AddRoomToMap(List<Vector3> map, float tileSize)
    {
        for (int x = -size.x; x < size.x; x++)
        {
            for (int z = -size.z; z < size.z; z++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    map.Add(position + (new Vector3(x, y, z) * tileSize));
                }
            }
        }
    }
}

public class EZDungeon : MonoBehaviour
{
    [SerializeField] string seed;
    [SerializeField] float tileSize = 16;

    [SerializeField] Vector3Int minRoomPosition = new Vector3Int(-30, 0, -30);
    [SerializeField] Vector3Int maxRoomPosition = new Vector3Int(30, 0, 30);

    [SerializeField] [Min(1)] Vector3Int minRoomSize = new Vector3Int(1,1,1);
    [SerializeField] [Min(1)] Vector3Int maxRoomSize = new Vector3Int(5,1,5);
    [SerializeField] [Min(1)] int numberOfRooms = 10;

    [SerializeField] Material floorMaterial;
    [SerializeField] Material wallMaterial;
    [SerializeField] Material ceilingMaterial;
    
    List<Vector3> pointMap = new List<Vector3>();

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
        floors.AddComponent<MeshFilter>();
        floors.AddComponent<MeshRenderer>();
        floors.GetComponent<MeshRenderer>().material = floorMaterial;
        floors.AddComponent<MeshCollider>();
        floors.isStatic = true;
        floors.transform.parent = transform;

        Random.InitState(seed.GetHashCode());

        CreateDungeon();
        CreateFloorMesh();
        CreateWallMesh();
        CreateCeilingMesh();
    }

    void CreateDungeon()
    {
        //Create Rooms
        EZRoom[] rooms = new EZRoom[numberOfRooms];
        for(int r = 0; r < numberOfRooms; r++)
        {
            rooms[r] = new EZRoom(RandomRoomPosition(minRoomPosition, maxRoomPosition), RandomRoomSize(minRoomSize, maxRoomSize), tileSize);
            rooms[r].AddRoomToMap(pointMap, tileSize);
        }
        
        //Place player in a room
        GameObject.FindGameObjectWithTag("Player").transform.position = rooms[0].position;

        //Create Hallways
        for(int r = 0; r < numberOfRooms; r++) 
        {
            if(r+1 < numberOfRooms)
            {
                CreateHallway(rooms[r].GetNearestExit(rooms[r + 1].position), rooms[r + 1].GetNearestExit(rooms[r + 1].position));
            }

        }



        //Remove Duplicate positions
        pointMap = pointMap.Distinct().ToList();
    }

    void CreateHallway(Vector3 start, Vector3 end)
    {
        Vector3 currentPos = start;
        pointMap.Add(currentPos);

        while(Vector3.Distance(currentPos, end) > 0)
        {
            //Get Possible Directions
            Vector3[] possibleDirections = 
            {
                (Vector3.forward * tileSize),
                (Vector3.back * tileSize),
                (Vector3.left * tileSize),
                (Vector3.right * tileSize),

                (new Vector3(0,1,1) * tileSize),
                (new Vector3(0,1,-1) * tileSize),
                (new Vector3(-1,1,0) * tileSize),
                (new Vector3(1,1,0) * tileSize),
                (new Vector3(0,-1,1) * tileSize),
                (new Vector3(0,-1,-1) * tileSize),
                (new Vector3(-1,-1,0) * tileSize),
                (new Vector3(1,-1,0) * tileSize),
            };

            //Calculate which direction To Go
            Vector3 chosenDirection = possibleDirections[0];
            foreach(Vector3 direction in possibleDirections) 
            {
                if(Vector3.Distance(currentPos + direction, end) < Vector3.Distance(currentPos + chosenDirection, end))
                {
                    chosenDirection = direction;
                }
            }

            currentPos += chosenDirection;
            pointMap.Add(currentPos);

        }
    }

    void CreateHallway2(Vector3 start, Vector3 end)
    {
        Vector3 currentPos = start;
        pointMap.Add(currentPos);

        
        while(currentPos.x != end.x)
        {
            if (currentPos.x < end.x)
            {
                currentPos.x += tileSize;
                pointMap.Add(currentPos);
            }
            else if (currentPos.x > end.x)
            {
                currentPos.x -= tileSize;
                pointMap.Add(currentPos);
            }
        }
        while (currentPos.z != end.z)
        {
            if (currentPos.z < end.z)
            {
                currentPos.z += tileSize;
                pointMap.Add(currentPos);
            }
            else if (currentPos.z > end.z)
            {
                currentPos.z -= tileSize;
                pointMap.Add(currentPos);
            }
        }
        while (currentPos.y != end.y)
        {
            if (currentPos.y < end.y)
            {
                currentPos.y += tileSize;
                pointMap.Add(currentPos);
            }
            else if (currentPos.y > end.y)
            {
                currentPos.y -= tileSize;
                pointMap.Add(currentPos);
            }
        }


    }


    void CreateWallBack(Vector3 position, float size)
    {
        wallVerts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * size + position);
        wallVerts.Add(new Vector3(0.5f, -0.5f, -0.5f) * size + position);
        wallVerts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * size + position);
        wallVerts.Add(new Vector3(0.5f, 0.5f, -0.5f) * size + position);

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

    void CreateWallFront(Vector3 position, float size)
    {
        wallVerts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * size + position);
        wallVerts.Add(new Vector3(0.5f, -0.5f, 0.5f) * size + position);
        wallVerts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * size + position);
        wallVerts.Add(new Vector3(0.5f, 0.5f, 0.5f) * size + position);

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

    void CreateWallRight(Vector3 position, float size)
    {
        wallVerts.Add(new Vector3(0.5f, 0.5f, 0.5f) * size + position);
        wallVerts.Add(new Vector3(0.5f, -0.5f, 0.5f) * size + position);
        wallVerts.Add(new Vector3(0.5f, -0.5f, -0.5f) * size + position);
        wallVerts.Add(new Vector3(0.5f, 0.5f, -0.5f) * size + position);

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

    void CreateWallLeft(Vector3 position, float size)
    {
        wallVerts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * size + position);
        wallVerts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * size + position);
        wallVerts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * size + position);
        wallVerts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * size + position);

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

    void CreateFloor(Vector3 position, float size)
    {
        floorVerts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * size + position);
        floorVerts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * size + position);
        floorVerts.Add(new Vector3(0.5f, -0.5f, -0.5f) * size + position);
        floorVerts.Add(new Vector3(0.5f, -0.5f, 0.5f) * size + position);

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

    void CreateCeiling(Vector3 position, float size)
    {
        ceilingVerts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * size + position);
        ceilingVerts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * size + position);
        ceilingVerts.Add(new Vector3(0.5f, 0.5f, -0.5f) * size + position);
        ceilingVerts.Add(new Vector3(0.5f, 0.5f, 0.5f) * size + position);

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
    
    void CreateFloorMesh()
    {
        //Initilize Mesh
        floorMesh = new Mesh();
        floors.GetComponent<MeshFilter>().mesh = floorMesh;

        //Populate Mesh Values
        for (int i = 0; i < pointMap.Count; i++)
        {
            if (!pointMap.Contains(pointMap[i] + Vector3.down * tileSize))
            {
                Vector3 pos = pointMap[i];
                CreateFloor(pos, tileSize);
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
        for (int i = 0; i < pointMap.Count; i++)
        {
            //Add Walls
            if (!pointMap.Contains(pointMap[i] + Vector3.forward * tileSize))
            {
                Vector3 pos = pointMap[i];
                CreateWallFront(pos, tileSize);
            }
            if (!pointMap.Contains(pointMap[i] + Vector3.back * tileSize))
            {
                Vector3 pos = pointMap[i];
                CreateWallBack(pos, tileSize);
            }
            if (!pointMap.Contains(pointMap[i] + Vector3.right * tileSize))
            {
                Vector3 pos = pointMap[i];
                CreateWallRight(pos, tileSize);
            }
            if (!pointMap.Contains(pointMap[i] + Vector3.left * tileSize))
            {
                Vector3 pos = pointMap[i];
                CreateWallLeft(pos, tileSize);
            }
        }

        //Draw Mesh
        wallMesh.Clear();
        wallMesh.vertices = wallVerts.ToArray();
        wallMesh.triangles = wallTris.ToArray();
        wallMesh.uv = wallUvs.ToArray();
        wallMesh.RecalculateNormals();
        wallMesh.RecalculateTangents();

        walls.GetComponent<MeshCollider>().sharedMesh = floorMesh;

    }

    void CreateCeilingMesh()
    {
        ceilingMesh = new Mesh();
        ceiling.GetComponent<MeshFilter>().mesh = ceilingMesh;

        //Populate Mesh Values
        for (int i = 0; i < pointMap.Count; i++)
        {
            if (!pointMap.Contains(pointMap[i] + Vector3.up * tileSize))
            {
                Vector3 pos = pointMap[i];
                CreateCeiling(pos, tileSize);
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

    Vector3 RandomRoomPosition(Vector3Int min, Vector3Int max)
    {
        int x = Random.Range(min.x, max.x);
        int y = (Random.Range(min.y, max.y) / (maxRoomSize.y + 1)) * (maxRoomSize.y + 1);
        int z = Random.Range(min.z, max.z);
        return new Vector3(x, y, z) * tileSize;
    }

    Vector3Int RandomRoomSize(Vector3Int min , Vector3Int max)
    {
        int x = Random.Range(min.x, max.x + 1);
        int y = Random.Range(min.y, max.y + 1);
        int z = Random.Range(min.z, max.z + 1);
        return new Vector3Int(x, y, z);
    }
}
