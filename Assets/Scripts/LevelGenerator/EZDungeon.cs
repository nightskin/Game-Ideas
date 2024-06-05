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

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class EZDungeon : MonoBehaviour
{
    [SerializeField] string seed;
    [SerializeField] float tileSize = 16;

    [SerializeField] GameObject walls;
    [SerializeField] GameObject floors;
    [SerializeField] GameObject ceilings;
    [SerializeField] GameObject ramps;

    [SerializeField] Vector3Int minRoomPosition = new Vector3Int(-30, 0, -30);
    [SerializeField] Vector3Int maxRoomPosition = new Vector3Int(30, 0, 30);

    [SerializeField] [Min(1)] Vector3Int minRoomSize = new Vector3Int(1,1,1);
    [SerializeField] [Min(1)] Vector3Int maxRoomSize = new Vector3Int(5,1,5);
    [SerializeField] [Min(1)] int numberOfRooms = 10;

    
    List<Vector3> pointMap = new List<Vector3>();
    
    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Color> colors = new List<Color>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        Random.InitState(seed.GetHashCode());

        CreateDungeon();

        if (!walls || !floors)
        {
            CreateMesh();
        }
        else 
        {
            AddTiles();
        }
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
                if (rooms[r].position.y != rooms[r + 1].position.y)
                {
                    CreateHallway(rooms[r].GetNearestExit(rooms[r + 1].position), rooms[r + 1].GetNearestExit(rooms[r + 1].position));
                }
                else
                {
                    CreateHallway2(rooms[r].GetNearestExit(rooms[r + 1].position), rooms[r + 1].GetNearestExit(rooms[r + 1].position));
                }
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
                (Vector3.up * tileSize),
                (Vector3.down * tileSize),
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

    void CreateQuadBack(Vector3 position, float size)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * size + position);

        tris.Add(1 + buffer);
        tris.Add(0 + buffer);
        tris.Add(2 + buffer);

        tris.Add(3 + buffer);
        tris.Add(0 + buffer);
        tris.Add(1 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        buffer += 4;
    }

    void CreateQuadFront(Vector3 position, float size)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * size + position);

        tris.Add(1 + buffer);
        tris.Add(2 + buffer);
        tris.Add(0 + buffer);

        tris.Add(0 + buffer);
        tris.Add(3 + buffer);
        tris.Add(1 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        buffer += 4;
    }

    void CreateQuadRight(Vector3 position, float size)
    {
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * size + position);

        tris.Add(0 + buffer);
        tris.Add(3 + buffer);
        tris.Add(1 + buffer);

        tris.Add(2 + buffer);
        tris.Add(1 + buffer);
        tris.Add(3 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        buffer += 4;
    }

    void CreateQuadLeft(Vector3 position, float size)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * size + position);

        tris.Add(3 + buffer);
        tris.Add(0 + buffer);
        tris.Add(1 + buffer);

        tris.Add(1 + buffer);
        tris.Add(2 + buffer);
        tris.Add(3 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        buffer += 4;
    }

    void CreateQuadBottom(Vector3 position, float size)
    {
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * size + position);

        tris.Add(0 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        tris.Add(1 + buffer);
        tris.Add(3 + buffer);
        tris.Add(2 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));


        buffer += 4;

    }

    void CreateQuadTop(Vector3 position, float size)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * size + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * size + position);

        tris.Add(1 + buffer);
        tris.Add(0 + buffer);
        tris.Add(2 + buffer);

        tris.Add(3 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));


        buffer += 4;

    }

    void CreateRamp(Vector3 position, Vector3 size, Quaternion rotation)
    {
        verts.Add(rotation * new Vector3(-0.5f, 0, -0.5f) + position);
        verts.Add(rotation * new Vector3(-0.5f, 0, 0.5f) + position);
        verts.Add(rotation * new Vector3(0.5f, 0, -0.5f) + position);
        verts.Add(rotation * new Vector3(0.5f, 0, 0.5f) + position);

        tris.Add(0 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        tris.Add(1 + buffer);
        tris.Add(3 + buffer);
        tris.Add(2 + buffer);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));


        buffer += 4;
    }

    void AddTiles()
    {
        for (int i = 0; i < pointMap.Count; i++)
        {
            //Add Floors and Ceilings
            if (!pointMap.Contains(pointMap[i] + Vector3.up * tileSize))
            {
                if(ceilings)
                {
                    Instantiate(ceilings, pointMap[i] + (Vector3.up * (tileSize / 2)), Quaternion.identity, transform);
                }
                else
                {
                    Instantiate(floors, pointMap[i] + (Vector3.up * (tileSize / 2)), Quaternion.identity, transform);
                }
            }
            else
            {
                if (pointMap.Contains(pointMap[i] + new Vector3(1,1,0) * tileSize))
                {
                    Instantiate(ramps, new Vector3(0, 0, 0), Quaternion.Euler(0,0,0), transform);
                }


            }
            if (!pointMap.Contains(pointMap[i] + Vector3.down * tileSize))
            {
                Instantiate(floors, pointMap[i] + Vector3.down * (tileSize / 2), Quaternion.identity, transform);
            }
            else
            {

            }

            //Add Walls
            if (!pointMap.Contains(pointMap[i] + Vector3.forward * tileSize))
            {
                Vector3 pos = pointMap[i] + new Vector3(0, 0, tileSize / 2);
                Instantiate(walls, pos, Quaternion.identity, transform);
            }

            if (!pointMap.Contains(pointMap[i] + Vector3.back * tileSize))
            {
                Vector3 pos = pointMap[i] + new Vector3(0, 0, -tileSize / 2);
                Instantiate(walls, pos, Quaternion.identity, transform);
            }

            if (!pointMap.Contains(pointMap[i] + Vector3.right * tileSize))
            {
                Vector3 pos = pointMap[i] + new Vector3(tileSize / 2, 0, 0);
                Instantiate(walls, pos, Quaternion.Euler(0, 90, 0), transform);
            }
            if (!pointMap.Contains(pointMap[i] + Vector3.left * tileSize))
            {
                Vector3 pos = pointMap[i] + new Vector3(-tileSize / 2, 0, 0);
                Instantiate(walls, pos, Quaternion.Euler(0, 90, 0), transform);
            }
        }
    }

    void CreateMesh()
    {
        //Initilize Mesh
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        //Populate Mesh Values
        for (int i = 0; i < pointMap.Count; i++)
        {
            //Add Walls
            if (!pointMap.Contains(pointMap[i] + Vector3.forward * tileSize))
            {
                Vector3 pos = pointMap[i] + new Vector3(0, -tileSize / 2, 0);
                CreateQuadFront(pos, tileSize);
            }
            if (!pointMap.Contains(pointMap[i] + Vector3.back * tileSize))
            {
                Vector3 pos = pointMap[i] + new Vector3(0, -tileSize / 2, 0);
                CreateQuadBack(pos, tileSize);
            }
            if (!pointMap.Contains(pointMap[i] + Vector3.right * tileSize))
            {
                Vector3 pos = pointMap[i] + new Vector3(0, -tileSize / 2, 0);
                CreateQuadRight(pos, tileSize);
            }
            if (!pointMap.Contains(pointMap[i] + Vector3.left * tileSize))
            {
                Vector3 pos = pointMap[i] + new Vector3(0, -tileSize / 2, 0);
                CreateQuadLeft(pos, tileSize);
            }
            
            //Add Floors and Ceilings
            if (!pointMap.Contains(pointMap[i] + Vector3.up * tileSize))
            {
                Vector3 pos = pointMap[i] + new Vector3(0, -tileSize / 2, 0);
                CreateQuadTop(pos, tileSize);
            }
            if (!pointMap.Contains(pointMap[i] + Vector3.down * tileSize))
            {
                Vector3 pos = pointMap[i] + new Vector3(0, -tileSize / 2, 0);
                CreateQuadBottom(pos, tileSize);
            }
        }

        //Draw Mesh
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = colors.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
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
