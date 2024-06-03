using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MapAlgorithm
{
    TinyKeep,
    RandomWalker,
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class LevelGenerator : MonoBehaviour
{
    [SerializeField] MapAlgorithm mapType;
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

    [SerializeField] bool threeDimensionalWalk;
    [SerializeField] int steps = 100;
    
    Vector3 walkerPosition = new Vector3(0, 0, 0);
    List<Vector3> positions = new List<Vector3>();
    
    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Color> colors = new List<Color>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        Random.InitState(seed.GetHashCode());

        if (mapType == MapAlgorithm.RandomWalker)
        {
            RandomWalk();
        }
        else if (mapType == MapAlgorithm.TinyKeep)
        {
            TinyKeep();
        }

        if (!walls || !floors)
        {
            CreateMesh();
        }
        else 
        {
            AddTiles();
        }
    }

    void RandomWalk()
    {
        positions.Add(walkerPosition);
        if(threeDimensionalWalk)
        {
            for (int step = 0; step < steps; step++)
            {
                int direction = Random.Range(0, 6);

                if (direction == 0)
                {
                    walkerPosition += Vector3.right * tileSize;
                }
                else if (direction == 1)
                {
                    walkerPosition += Vector3.left * tileSize;
                }
                else if (direction == 2)
                {
                    walkerPosition += Vector3.forward * tileSize;
                }
                else if (direction == 3)
                {
                    walkerPosition += Vector3.back * tileSize;
                }
                else if(direction == 4)
                {
                    walkerPosition += Vector3.up * tileSize;
                }
                else if(direction == 5)
                {
                    walkerPosition += Vector3.down * tileSize;
                }

                positions.Add(walkerPosition);

            }
        }
        else
        {
            for (int step = 0; step < steps; step++)
            {
                int xz = Random.Range(0, 4);

                if (xz == 0)
                {
                    walkerPosition += Vector3.right * tileSize;
                }
                else if (xz == 1)
                {
                    walkerPosition += Vector3.left * tileSize;
                }
                else if (xz == 2)
                {
                    walkerPosition += Vector3.forward * tileSize;
                }
                else if (xz == 3)
                {
                    walkerPosition += Vector3.back * tileSize;
                }

                positions.Add(walkerPosition);

            }
        }


        positions = positions.Distinct().ToList();
    }

    void TinyKeep()
    {
        //Create Rooms
        Vector3[] roomPositions = new Vector3[numberOfRooms];
        for(int r = 0; r < numberOfRooms; r++)
        {
            roomPositions[r] = RandomRoomPosition(minRoomPosition, maxRoomPosition);
        }
        for(int r = 0; r < roomPositions.Length; r++)
        {
            CreateRoom(roomPositions[r], RandomRoomSize(minRoomSize, maxRoomSize));
        }
        
        //Place player in a room
        GameObject.FindGameObjectWithTag("Player").transform.position = roomPositions[0];

        //Create Hallways
        for(int i = 0; i < roomPositions.Length; i++)
        {
            if(i+1 < roomPositions.Length - 1) 
            {
                if (roomPositions[i].y != roomPositions[i+1].y) 
                {
                    CreateHallway(roomPositions[i], roomPositions[i + 1]);
                }
                else
                {
                    CreateHallway2(roomPositions[i], roomPositions[i + 1]);
                }

            }
        }

        //Create Last Hallway
        if (roomPositions[roomPositions.Length - 2].y != roomPositions[roomPositions.Length-1].y)
        {
            CreateHallway(roomPositions[roomPositions.Length - 2], roomPositions[roomPositions.Length - 1]);
        }
        else
        {
            CreateHallway2(roomPositions[roomPositions.Length - 2], roomPositions[roomPositions.Length - 1]);
        }


        //Remove Duplicate positions
        positions = positions.Distinct().ToList();
    }

    void CreateRoom(Vector3 position, Vector3Int size)
    {
        for (int x = -size.x; x < size.x; x++)
        {
            for (int z = -size.z; z < size.z; z++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    positions.Add(position + (new Vector3(x, y, z) * tileSize));
                }
            }
        }
    }

    void CreateHallway(Vector3 start, Vector3 end)
    {
        Vector3 currentPos = start;
        positions.Add(currentPos);

        while(Vector3.Distance(currentPos, end) > 0)
        {
            //Get Possible Directions
            Vector3[] possibleDirections = 
            {
                (Vector3.forward * tileSize),
                (Vector3.back * tileSize),
                (Vector3.left * tileSize),
                (Vector3.right * tileSize),

                (new Vector3(0, 1, 1) * tileSize),
                (new Vector3(0, 1, -1) * tileSize),
                (new Vector3(-1, 1, 0) * tileSize),
                (new Vector3(1, 1, 0) * tileSize),
                
                (new Vector3(0, -1, 1) * tileSize),
                (new Vector3(0, -1, -1) * tileSize),
                (new Vector3(-1, -1, 0) * tileSize),
                (new Vector3(1, -1, 0) * tileSize),
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
            positions.Add(currentPos);


        }
    }

    void CreateHallway2(Vector3 start, Vector3 end)
    {
        Vector3 currentPos = start;
        positions.Add(currentPos);

        
        while(currentPos.x != end.x)
        {
            if (currentPos.x < end.x)
            {
                currentPos.x += tileSize;
                positions.Add(currentPos);
            }
            else if (currentPos.x > end.x)
            {
                currentPos.x -= tileSize;
                positions.Add(currentPos);
            }
        }
        while (currentPos.y != end.y)
        {
            if (currentPos.y < end.y)
            {
                currentPos.y += tileSize;
                positions.Add(currentPos);
            }
            else if (currentPos.y > end.y)
            {
                currentPos.y -= tileSize;
                positions.Add(currentPos);
            }
        }
        while (currentPos.z != end.z)
        {
            if (currentPos.z < end.z)
            {
                currentPos.z += tileSize;
                positions.Add(currentPos);
            }
            else if (currentPos.z > end.z)
            {
                currentPos.z -= tileSize;
                positions.Add(currentPos);
            }
        }

    }

    void CreateQuadBack(Vector3 position, float size, Color color)
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

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;
    }

    void CreateQuadFront(Vector3 position, float size, Color color)
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

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;
    }

    void CreateQuadRight(Vector3 position, float size, Color color)
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

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;
    }

    void CreateQuadLeft(Vector3 position, float size, Color color)
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

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;
    }

    void CreateQuadBottom(Vector3 position, float size, Color color)
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

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;

    }

    void CreateQuadTop(Vector3 position, float size, Color color)
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

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        buffer += 4;

    }

    void AddTiles()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            //Add Walls
            if (!positions.Contains(positions[i] + Vector3.forward * tileSize))
            {
                Vector3 pos = positions[i] + new Vector3(0, 0, tileSize / 2);
                Instantiate(walls, pos, Quaternion.identity, transform);
            }
            if (!positions.Contains(positions[i] + Vector3.back * tileSize))
            {
                Vector3 pos = positions[i] + new Vector3(0, 0, -tileSize / 2);
                Instantiate(walls, pos, Quaternion.identity, transform);
            }
            if (!positions.Contains(positions[i] + Vector3.right * tileSize))
            {
                Vector3 pos = positions[i] + new Vector3(tileSize / 2, 0, 0);
                Instantiate(walls, pos, Quaternion.Euler(0, 90, 0), transform);
            }
            if (!positions.Contains(positions[i] + Vector3.left * tileSize))
            {
                Vector3 pos = positions[i] + new Vector3(-tileSize / 2, 0, 0);
                Instantiate(walls, pos, Quaternion.Euler(0, 90, 0), transform);
            }
            //Add Floors and Ceilings
            if (!positions.Contains(positions[i] + Vector3.up * tileSize))
            {
                if(ceilings)
                {
                    Instantiate(ceilings, positions[i] + (Vector3.up * (tileSize / 2)), Quaternion.identity, transform);
                }
                else
                {
                    Instantiate(floors, positions[i] + (Vector3.up * (tileSize / 2)), Quaternion.identity, transform);
                }

            }
            if (!positions.Contains(positions[i] + Vector3.down * tileSize))
            {
                Instantiate(floors, positions[i] + Vector3.down * (tileSize / 2), Quaternion.identity, transform);
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
        for (int i = 0; i < positions.Count; i++)
        {
            //Add Walls
            if (!positions.Contains(positions[i] + Vector3.forward * tileSize))
            {
                Vector3 pos = positions[i] + new Vector3(0, -tileSize / 2, 0);
                CreateQuadFront(pos, tileSize, Color.white);
            }
            if (!positions.Contains(positions[i] + Vector3.back * tileSize))
            {
                Vector3 pos = positions[i] + new Vector3(0, -tileSize / 2, 0);
                CreateQuadBack(pos, tileSize, Color.white);
            }
            if (!positions.Contains(positions[i] + Vector3.right * tileSize))
            {
                Vector3 pos = positions[i] + new Vector3(0, -tileSize / 2, 0);
                CreateQuadRight(pos, tileSize, Color.white);
            }
            if (!positions.Contains(positions[i] + Vector3.left * tileSize))
            {
                Vector3 pos = positions[i] + new Vector3(0, -tileSize / 2, 0);
                CreateQuadLeft(pos, tileSize, Color.white);
            }
            
            //Add Floors and Ceilings
            if (!positions.Contains(positions[i] + Vector3.up * tileSize))
            {
                Vector3 pos = positions[i] + new Vector3(0, -tileSize / 2, 0);
                CreateQuadTop(pos, tileSize, Color.white);
            }
            if (!positions.Contains(positions[i] + Vector3.down * tileSize))
            {
                Vector3 pos = positions[i] + new Vector3(0, -tileSize / 2, 0);
                CreateQuadBottom(pos, tileSize, Color.white);
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
