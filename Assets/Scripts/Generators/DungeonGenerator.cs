using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class DungeonGenerator : MonoBehaviour
{
    [Header("Default Parameters")]
    [Tooltip("Player GameObject That will be placed in the level on Runtime")] public Transform player;

    [Tooltip("Determines max size of the level")] public Vector3Int size = Vector3Int.one * 100;
    public string seed = string.Empty;
    public enum LevelGenerationAlgorithm
    {
        RANDOM_WALKER,
        TINY_KEEP,
        RANDOM_KEEP,
    }
    [SerializeField] LevelGenerationAlgorithm levelGeneration = LevelGenerationAlgorithm.RANDOM_KEEP;
    public enum MeshGenerationAlgorithm
    {
        VOXEL_MESH,
        MARCHING_CUBES,
        MARCHING_CUBES_SMOOTH,
    }
    [SerializeField] float incrementLevel = 0.01f;
    [SerializeField] MeshGenerationAlgorithm meshGeneration = MeshGenerationAlgorithm.MARCHING_CUBES;
    [SerializeField][Min(1)] int hallwaySize = 1;
    float[,,] grid = null;
    List<Vector3> verts;
    List<Vector2> uvs;
    List<int> tris;
    int buffer;
    Mesh mesh;

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
    [SerializeField] bool useIndirectHallways = false;
    
    [Header("Debug")]
    [SerializeField] bool showBounds = false;
    [SerializeField] Color boundColor = Color.rebeccaPurple;

    void OnDrawGizmos()
    {
        if (showBounds)
        {
            Gizmos.color = boundColor;
            Gizmos.DrawWireCube(transform.position + ((Vector3)size / 2 * Voxel.size), (Vector3)size * Voxel.size);
        }
    }

    void Start()
    {
        Init();
        GenerateDungeon(levelGeneration);
        GenerateMesh();
        PlacePlayer();
    }

    void Init()
    {
        if (!player) player = GameObject.FindWithTag("Player").transform;
        if (seed == string.Empty) seed = System.DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());

        grid = new float[size.x, size.y, size.z];
        verts = new List<Vector3>();
        uvs = new List<Vector2>();
        tris = new List<int>();
        buffer = 0;
    }

    void GenerateDungeon(LevelGenerationAlgorithm algorithm)
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
                    if (cell.x + x >= size.x - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= size.y - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= size.z - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    

                    if (grid[cell.x + x, cell.y + y, cell.z + z] < Voxel.isoLevel)
                    {
                        grid[cell.x + x, cell.y + y, cell.z + z] = Voxel.isoLevel;
                    }
                    else
                    {
                        grid[cell.x + x, cell.y + y, cell.z + z] += incrementLevel;
                    }
                }
            }
        }
    }
    
    void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;


        if (meshGeneration == MeshGenerationAlgorithm.VOXEL_MESH)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        if (grid[x, y, z] > Voxel.isoLevel)
                        {
                            if (y > 0)
                            {
                                if (grid[x, y - 1, z] <= Voxel.isoLevel)
                                {
                                    DrawQuadBottom(new Vector3(x, y, z) * Voxel.size);
                                }
                            }
                            if (y < size.y - 1)
                            {
                                if (grid[x, y + 1, z] <= Voxel.isoLevel)
                                {
                                    DrawQuadTop(new Vector3(x, y, z) * Voxel.size);
                                }
                            }
                            if (x > 0)
                            {
                                if (grid[x - 1, y, z] <= Voxel.isoLevel)
                                {
                                    DrawQuadLeft(new Vector3(x, y, z) * Voxel.size);
                                }
                            }
                            if (x < size.x - 1)
                            {
                                if (grid[x + 1, y, z] <= Voxel.isoLevel)
                                {
                                    DrawQuadRight(new Vector3(x, y, z) * Voxel.size);
                                }
                            }
                            if (z > 0)
                            {
                                if (grid[x, y, z - 1] <= Voxel.isoLevel)
                                {
                                    DrawQuadBack(new Vector3(x, y, z) * Voxel.size);
                                }
                            }
                            if (z < size.z - 1)
                            {
                                if (grid[x, y, z + 1] <= Voxel.isoLevel)
                                {
                                    DrawQuadFront(new Vector3(x, y, z) * Voxel.size);
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            for (int x = 0; x < size.x - 1; x++)
            {
                for (int y = 0; y < size.y - 1; y++)
                {
                    for (int z = 0; z < size.z - 1; z++)
                    {

                        float[] values = new float[]
                        {
                            grid[x,y,z+1],
                            grid[x+1,y,z+1],
                            grid[x+1,y,z],
                            grid[x,y,z],
                            grid[x,y+1,z+1],
                            grid[x+1,y+1,z+1],
                            grid[x+1,y+1,z],
                            grid[x,y+1,z],
                        };

                        Vector3[] points = new Vector3[]
                        {
                            new Vector3(x,y,z+1) * Voxel.size,
                            new Vector3(x+1,y,z+1) * Voxel.size,
                            new Vector3(x+1,y,z) * Voxel.size,
                            new Vector3(x,y,z) * Voxel.size,
                            new Vector3(x,y+1,z+1) * Voxel.size,
                            new Vector3(x+1,y+1,z+1) * Voxel.size,
                            new Vector3(x+1,y+1,z) * Voxel.size,
                            new Vector3(x,y+1,z) * Voxel.size,
                        };

                        int cubeIndex = Voxel.GetState(values);

                        Vector3[] triVerts = new Vector3[3];
                        int triIndex = 0;

                        int[] triangulation = MarchingCubesTables.triTable[cubeIndex];
                        foreach (int edgeIndex in triangulation)
                        {
                            if (edgeIndex > -1)
                            {
                                int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                                int b = MarchingCubesTables.edgeConnections[edgeIndex][1];

                                Vector3 vertexPos = Vector3.Lerp(points[a], points[b], 0.5f);
                                if(meshGeneration == MeshGenerationAlgorithm.MARCHING_CUBES_SMOOTH) vertexPos = Voxel.LerpPoint(values[a], values[b], points[a], points[b]);
                                
                                verts.Add(vertexPos);
                                tris.Add(buffer);

                                if (triIndex == 0)
                                {
                                    triVerts[0] = vertexPos;
                                    triIndex++;
                                }
                                else if (triIndex == 1)
                                {
                                    triVerts[1] = vertexPos;
                                    triIndex++;
                                }
                                else if (triIndex == 2)
                                {
                                    triVerts[2] = vertexPos;
                                    uvs.AddRange(Voxel.GetUVs(triVerts[0], triVerts[1], triVerts[2]));
                                    triIndex = 0;
                                }

                                buffer++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void RandomWalker()
    {
        Vector3Int currentIndex = size / 2;

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
            int xi = UnityEngine.Random.Range(0, size.x);
            int yi = UnityEngine.Random.Range(0, size.y);
            int zi = UnityEngine.Random.Range(0, size.z);

            Vector3Int currentIndex = new Vector3Int(xi, yi, zi);
            entrances.Add(currentIndex);
            for (int s = 0; s < numberOfSteps; s++)
            {
                int x = UnityEngine.Random.Range(-1, 2);
                int y = 0;
                if (walk3D) y = UnityEngine.Random.Range(-1, 2);
                int z = UnityEngine.Random.Range(-1, 2);

                if (x == -1 && currentIndex.x <= 0) x = 1;
                if (x == 1 && currentIndex.x >= size.x - 1) x = -1;

                if (z == -1 && currentIndex.z <= 0) z = 1;
                if (z == 1 && currentIndex.z >= size.z - 1) z = -1;

                if (y == -1 && currentIndex.y <= 0) y = 1;
                if (y == 1 && currentIndex.y >= size.y - 1) y = -1;


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
            if (useIndirectHallways)
            {
                GenerateHallway2(start, end);
            }
            else
            {
                GenerateHallway(start, end);
            }
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

            int rx = UnityEngine.Random.Range(roomSizeX, size.x - roomSizeX);
            int ry = UnityEngine.Random.Range(ceilngHeight, size.y - ceilngHeight);
            int rz = UnityEngine.Random.Range(roomSizeZ, size.z - roomSizeZ);
            Vector3Int roomPosition = new Vector3Int(rx, ry, rz);
            ActivateBox(roomPosition, roomSizeX, ceilngHeight, roomSizeZ);
            pointsOfInterest.Add(roomPosition);
        }

        //Create Hallways
        for (int r = 0; r < numberOfRooms - 1; r++)
        {
            Vector3Int start = pointsOfInterest[r];
            Vector3Int end = pointsOfInterest[r + 1];
            
            if (useIndirectHallways)
            {
                GenerateHallway2(start, end);
            }
            else
            {
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
                Vector3Int.up,
                Vector3Int.down,
                Vector3Int.forward,
                Vector3Int.back,
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

    void GenerateHallway2(Vector3Int start, Vector3Int end)
    {
        Vector3Int currentPos = start;

        while (currentPos.x != end.x)
        {
            if (currentPos.x < end.x)
            {
                currentPos.x++;
            }
            else if (currentPos.x > end.x)
            {
                currentPos.x--;
            }
            ActivateBox(currentPos, hallwaySize, hallwaySize, hallwaySize);
        }

        while (currentPos.z != end.z)
        {
            if (currentPos.z < end.z)
            {
                currentPos.z++;
            }
            else if (currentPos.z > end.z)
            {
                currentPos.z--;
            }
            ActivateBox(currentPos, hallwaySize, hallwaySize, hallwaySize);
        }

        while (currentPos.y != end.y)
        {
            if (currentPos.y < end.y)
            {
                currentPos.y++;
            }
            else if (currentPos.y > end.y)
            {
                currentPos.y--;
            }
            ActivateBox(currentPos, hallwaySize, hallwaySize, hallwaySize);
        }
    }

    void DrawQuadBottom(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) *  Voxel.size + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) *   Voxel.size + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) *  Voxel.size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * Voxel.size + position);

        tris.Add(buffer + 0);
        tris.Add(buffer + 1);
        tris.Add(buffer + 2);
        tris.Add(buffer + 3);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadTop(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) *  Voxel.size + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) *   Voxel.size + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) *  Voxel.size + position);
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * Voxel.size + position);

        tris.Add(buffer + 2);
        tris.Add(buffer + 1);
        tris.Add(buffer + 0);

        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 3);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadFront(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) *  Voxel.size + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) *   Voxel.size + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) *  Voxel.size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * Voxel.size + position);

        tris.Add(buffer + 0);
        tris.Add(buffer + 1);
        tris.Add(buffer + 2);

        tris.Add(buffer + 3);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadBack(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) *  Voxel.size + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) *   Voxel.size + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) *  Voxel.size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * Voxel.size + position);

        tris.Add(buffer + 2);
        tris.Add(buffer + 1);
        tris.Add(buffer + 0);

        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 3);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadLeft(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) *  Voxel.size + position);
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) *   Voxel.size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) *  Voxel.size + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * Voxel.size + position);

        tris.Add(buffer + 0);
        tris.Add(buffer + 1);
        tris.Add(buffer + 2);

        tris.Add(buffer + 3);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void DrawQuadRight(Vector3 position)
    {
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) *  Voxel.size + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) *   Voxel.size + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) *  Voxel.size + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * Voxel.size + position);

        tris.Add(buffer + 2);
        tris.Add(buffer + 1);
        tris.Add(buffer + 0);

        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 3);

        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));

        buffer += 4;
    }

    void PlacePlayer()
    {
        if (player)
        {

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        if (grid[x, y, z] > Voxel.isoLevel)
                        {
                            player.transform.position = new Vector3(x, y, z) * Voxel.size;
                            player.transform.position += Vector3.up * size.y * Voxel.size;
                            if (Physics.Raycast(player.transform.position, Vector3.down, out RaycastHit hit))
                            {
                                player.position = hit.point;
                            }
                            return;
                        }
                    }
                }
            }


        }
    }
    
}