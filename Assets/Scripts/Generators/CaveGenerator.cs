using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class CaveGenerator : MonoBehaviour
{
    [Header("Default Parameters")]
    [Tooltip("Player GameObject That will be placed in the level on Runtime")] public Transform player;

    [Tooltip("Determines max size of the level")][Min(1)] public Vector3Int gridSize = Vector3Int.one * 100;
    [Tooltip("Controls how far apart everything is")][Min(0)] public float tileSize = 5;
    public string seed = string.Empty;
    public enum LevelGenerationAlgorithm
    {
        RANDOM_WALKER,
        RANDOM_KEEP,
    }
    [SerializeField] LevelGenerationAlgorithm levelGeneration = LevelGenerationAlgorithm.RANDOM_KEEP;
    public enum MeshGenerationAlgorithm
    {
        VOXEL_MESH,
        MARCHING_CUBES,
        MARCHING_CUBES_SMOOTH,
    }
    [SerializeField] float isoLevel = 0.1f;
    [SerializeField] float incrementValue = 0.01f;
    [SerializeField] MeshGenerationAlgorithm meshGeneration = MeshGenerationAlgorithm.MARCHING_CUBES;

    float[,,] grid = null;
    List<Vector3> verts;
    List<Vector2> uvs;
    List<int> tris;
    int buffer;
    Mesh mesh;

    Noise noise;

    [Space]
    [Header("RANDOM_WALKER Parameters")]
    [SerializeField][Min(1)] int numberOfSteps = 200;
    [SerializeField] bool walk3D = false;

    [Space]
    [Header("TINY_KEEP Parameters")]
    [SerializeField][Min(1)] int numberOfRooms = 2;
    [SerializeField][Min(1)] int hallwaySize = 1;
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
            Gizmos.DrawWireCube(transform.position + ((Vector3)gridSize / 2 * tileSize), (Vector3)gridSize * tileSize);
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
        if (seed == string.Empty) seed = DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());
        noise = new Noise(seed.GetHashCode());

        grid = new float[gridSize.x, gridSize.y, gridSize.z];
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
        else if (algorithm == LevelGenerationAlgorithm.RANDOM_KEEP)
        {
            RandomKeep(numberOfRooms);
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
                    if (cell.x + x >= gridSize.x - 1 || cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= gridSize.y - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= gridSize.z - 1 || cell.z + z <= 0)
                    {
                        continue;
                    }

                    if (meshGeneration == MeshGenerationAlgorithm.MARCHING_CUBES_SMOOTH)
                    {

                        if (grid[cell.x + x, cell.y + y, cell.z + z] + incrementValue <= isoLevel)
                        {
                            grid[cell.x + x, cell.y + y, cell.z + z] = isoLevel;
                        }
                        else
                        {
                            grid[cell.x + x, cell.y + y, cell.z + z] += incrementValue;
                        }
                    }
                    else
                    {
                        grid[cell.x + x, cell.y + y, cell.z + z] = 1;
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

        if (meshGeneration == MeshGenerationAlgorithm.MARCHING_CUBES || meshGeneration == MeshGenerationAlgorithm.MARCHING_CUBES_SMOOTH)
        {
            for (int x = 0; x < gridSize.x - 1; x++)
            {
                for (int y = 0; y < gridSize.y - 1; y++)
                {
                    for (int z = 0; z < gridSize.z - 1; z++)
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
                            new Vector3(x,y,z+1) * tileSize,
                            new Vector3(x+1,y,z+1) * tileSize,
                            new Vector3(x+1,y,z) * tileSize,
                            new Vector3(x,y,z) * tileSize,
                            new Vector3(x,y+1,z+1) * tileSize,
                            new Vector3(x+1,y+1,z+1) * tileSize,
                            new Vector3(x+1,y+1,z) * tileSize,
                            new Vector3(x,y+1,z) * tileSize,
                        };

                        int cubeIndex = GetState(values);

                        Vector3[] triVerts = new Vector3[3];
                        int triIndex = 0;

                        int[] triangulation = MarchingCubesTables.triTable[cubeIndex];
                        foreach (int edgeIndex in triangulation)
                        {
                            if (edgeIndex > -1)
                            {
                                int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                                int b = MarchingCubesTables.edgeConnections[edgeIndex][1];

                                Vector3 vertexPos = LerpPoint(values[a], values[b], points[a], points[b]);
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
                                    uvs.AddRange(GetUVs(triVerts[0], triVerts[1], triVerts[2]));
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
        else if (meshGeneration == MeshGenerationAlgorithm.VOXEL_MESH)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        if (grid[x, y, z] > isoLevel)
                        {
                            if (y > 0)
                            {
                                if (grid[x, y - 1, z] <= isoLevel)
                                {
                                    DrawQuadBottom(new Vector3(x, y, z) * tileSize);
                                }
                            }
                            if (y < gridSize.y - 1)
                            {
                                if (grid[x, y + 1, z] <= isoLevel)
                                {
                                    DrawQuadTop(new Vector3(x, y, z) * tileSize);
                                }
                            }
                            if (x > 0)
                            {
                                if (grid[x - 1, y, z] <= isoLevel)
                                {
                                    DrawQuadLeft(new Vector3(x, y, z) * tileSize);
                                }
                            }
                            if (x < gridSize.x - 1)
                            {
                                if (grid[x + 1, y, z] <= isoLevel)
                                {
                                    DrawQuadRight(new Vector3(x, y, z) * tileSize);
                                }
                            }
                            if (z > 0)
                            {
                                if (grid[x, y, z - 1] <= isoLevel)
                                {
                                    DrawQuadBack(new Vector3(x, y, z) * tileSize);
                                }
                            }
                            if (z < gridSize.z - 1)
                            {
                                if (grid[x, y, z + 1] <= isoLevel)
                                {
                                    DrawQuadFront(new Vector3(x, y, z) * tileSize);
                                }
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
        Vector3Int currentIndex = gridSize / 2;

            for (int step = 0; step < numberOfSteps; step++)
            {
                int x = UnityEngine.Random.Range(-1, 2);
                int y = 0;
                if (walk3D) y = UnityEngine.Random.Range(-1, 2);
                int z = UnityEngine.Random.Range(-1, 2);

                currentIndex += new Vector3Int(x, y, z);
                ActivateBox(currentIndex, hallwaySize, hallwaySize, hallwaySize);
            }
    }

    void RandomKeep(int numberOfRooms)
    {
        //Create Rooms
        List<Vector3Int> entrances = new List<Vector3Int>();
        List<Vector3Int> exits = new List<Vector3Int>();
        for (int r = 0; r < numberOfRooms; r++)
        {
            int xi = UnityEngine.Random.Range(0, gridSize.x);
            int yi = UnityEngine.Random.Range(0, gridSize.y);
            int zi = UnityEngine.Random.Range(0, gridSize.z);

            Vector3Int currentIndex = new Vector3Int(xi, yi, zi);
            entrances.Add(currentIndex);
            for (int s = 0; s < numberOfSteps; s++)
            {
                int x = UnityEngine.Random.Range(-1, 2);
                int y = 0;
                if (walk3D) y = UnityEngine.Random.Range(-1, 2);
                int z = UnityEngine.Random.Range(-1, 2);

                if (x == -1 && currentIndex.x <= 0) x = 1;
                if (x == 1 && currentIndex.x >= gridSize.x - 1) x = -1;

                if (z == -1 && currentIndex.z <= 0) z = 1;
                if (z == 1 && currentIndex.z >= gridSize.z - 1) z = -1;

                if (y == -1 && currentIndex.y <= 0) y = 1;
                if (y == 1 && currentIndex.y >= gridSize.y - 1) y = -1;


                currentIndex += new Vector3Int(x, y, z);
                ActivateBox(currentIndex, hallwaySize, ceilngHeight, hallwaySize);

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
            ActivateBox(currentPos, hallwaySize, ceilngHeight, hallwaySize);
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
            ActivateBox(currentPos, hallwaySize, ceilngHeight, hallwaySize);
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
            ActivateBox(currentPos, hallwaySize, ceilngHeight, hallwaySize);
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
            ActivateBox(currentPos, hallwaySize, ceilngHeight, hallwaySize);
        }
    }

    Vector2[] GetUVs(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 s1 = b - a;
        Vector3 s2 = c - a;
        Vector3 norm = Vector3.Cross(s1, s2).normalized; // the normal

        norm.x = Mathf.Abs(norm.x);
        norm.y = Mathf.Abs(norm.y);
        norm.z = Mathf.Abs(norm.z);

        Vector2[] uvs = new Vector2[3];
        if (norm.x >= norm.z && norm.x >= norm.y) // x plane
        {
            uvs[0] = new Vector2(a.z, a.y) / tileSize;
            uvs[1] = new Vector2(b.z, b.y) / tileSize;
            uvs[2] = new Vector2(c.z, c.y) / tileSize;
        }
        else if (norm.z >= norm.x && norm.z >= norm.y) // z plane
        {
            uvs[0] = new Vector2(a.x, a.y) / tileSize;
            uvs[1] = new Vector2(b.x, b.y) / tileSize;
            uvs[2] = new Vector2(c.x, c.y) / tileSize;
        }
        else if (norm.y >= norm.x && norm.y >= norm.z) // y plane
        {
            uvs[0] = new Vector2(a.x, a.z) / tileSize;
            uvs[1] = new Vector2(b.x, b.z) / tileSize;
            uvs[2] = new Vector2(c.x, c.z) / tileSize;
        }

        return uvs;
    }

    void DrawQuadBottom(Vector3 position)
    {
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * tileSize + position);

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
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * tileSize + position);

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
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * tileSize + position);

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
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * tileSize + position);

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
        verts.Add(new Vector3(-0.5f, 0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, 0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(-0.5f, -0.5f, -0.5f) * tileSize + position);

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
        verts.Add(new Vector3(0.5f, 0.5f, -0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, 0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, 0.5f) * tileSize + position);
        verts.Add(new Vector3(0.5f, -0.5f, -0.5f) * tileSize + position);

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

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        if (grid[x, y, z] > isoLevel)
                        {
                            player.transform.position = new Vector3(x, y, z) * tileSize;
                            player.transform.position += Vector3.up * gridSize.y * tileSize;
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

    int GetState(float[] points)
    {
        int state = 0;
        if (points[0] >= isoLevel) state |= 1;
        if (points[1] >= isoLevel) state |= 2;
        if (points[2] >= isoLevel) state |= 4;
        if (points[3] >= isoLevel) state |= 8;
        if (points[4] >= isoLevel) state |= 16;
        if (points[5] >= isoLevel) state |= 32;
        if (points[6] >= isoLevel) state |= 64;
        if (points[7] >= isoLevel) state |= 128;
        return state;
    }

    Vector3 LerpPoint(float v1, float v2, Vector3 pos1, Vector3 pos2)
    {
        float amount = (isoLevel - v1) / (v2 - v1);
        return Vector3.Lerp(pos1, pos2, amount);
    }

}