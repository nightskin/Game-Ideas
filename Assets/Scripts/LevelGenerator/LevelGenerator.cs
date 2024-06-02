using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MapAlgorithm
{
    RandomWalker,
    TinyKeep
}

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] MapAlgorithm mapType;
    [SerializeField] string seed;
    [SerializeField] float stepSize = 2;

    [SerializeField] GameObject wallTile;
    [SerializeField] GameObject floorTile;

    [SerializeField] Vector3Int minRoomPosition;
    [SerializeField] Vector3Int maxRoomPosition;

    [SerializeField] [Min(1)] Vector3Int minRoomSize = Vector3Int.one;
    [SerializeField] [Min(1)] Vector3Int maxRoomSize = Vector3Int.one * 10;
    [SerializeField] [Min(1)] int numberOfRooms = 10;

    [SerializeField] bool threeDimensionalWalk;
    [SerializeField] int steps = 100;
    
    Vector3 walkerPosition = new Vector3(0, 0, 0);
    List<Vector3> positions = new List<Vector3>();


    void Start()
    {
        Random.InitState(seed.GetHashCode());

        if(mapType == MapAlgorithm.RandomWalker ) 
        {
            RandomWalk();
            AddTiles();
        }
        else if(mapType == MapAlgorithm.TinyKeep)
        {
            TinyKeep();
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
                    walkerPosition += Vector3.right * stepSize;
                }
                else if (direction == 1)
                {
                    walkerPosition += Vector3.left * stepSize;
                }
                else if (direction == 2)
                {
                    walkerPosition += Vector3.forward * stepSize;
                }
                else if (direction == 3)
                {
                    walkerPosition += Vector3.back * stepSize;
                }
                else if(direction == 4)
                {
                    walkerPosition += Vector3.up * stepSize;
                }
                else if(direction == 5)
                {
                    walkerPosition += Vector3.down * stepSize;
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
                    walkerPosition += Vector3.right * stepSize;
                }
                else if (xz == 1)
                {
                    walkerPosition += Vector3.left * stepSize;
                }
                else if (xz == 2)
                {
                    walkerPosition += Vector3.forward * stepSize;
                }
                else if (xz == 3)
                {
                    walkerPosition += Vector3.back * stepSize;
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
                CreateHallway2(roomPositions[i], roomPositions[i + 1]);
            }
        }
        CreateHallway2(roomPositions[roomPositions.Length - 2], roomPositions[roomPositions.Length - 1]);

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
                    positions.Add(position + (new Vector3(x, y, z) * stepSize));
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
                (Vector3.forward * stepSize),
                (Vector3.back * stepSize),
                (Vector3.left * stepSize),
                (Vector3.right * stepSize),

                (Vector3.up * stepSize),
                (Vector3.down * stepSize),

                //(new Vector3(0, 1, 1) * stepSize),
                //(new Vector3(0, 1, -1) * stepSize),
                //(new Vector3(-1, 1, 0) * stepSize),
                //(new Vector3(1, 1, 0) * stepSize),
                //
                //(new Vector3(0, -1, 1) * stepSize),
                //(new Vector3(0, -1, -1) * stepSize),
                //(new Vector3(-1, -1, 0) * stepSize),
                //(new Vector3(1, -1, 0) * stepSize),
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
                currentPos.x += stepSize;
                positions.Add(currentPos);
            }
            else if (currentPos.x > end.x)
            {
                currentPos.x -= stepSize;
                positions.Add(currentPos);
            }
        }
        while (currentPos.y != end.y)
        {
            if (currentPos.y < end.y)
            {
                currentPos.y += stepSize;
                positions.Add(currentPos);
            }
            else if (currentPos.y > end.y)
            {
                currentPos.y -= stepSize;
                positions.Add(currentPos);
            }
        }
        while (currentPos.z != end.z)
        {
            if (currentPos.z < end.z)
            {
                currentPos.z += stepSize;
                positions.Add(currentPos);
            }
            else if (currentPos.z > end.z)
            {
                currentPos.z -= stepSize;
                positions.Add(currentPos);
            }
        }

    }

    void AddTiles()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            //Add Walls
            if (!positions.Contains(positions[i] + Vector3.forward * stepSize))
            {
                Vector3 pos = positions[i] + new Vector3(0, 0, stepSize / 2);
                Instantiate(wallTile, pos, Quaternion.identity, transform);
            }
            if (!positions.Contains(positions[i] + Vector3.back * stepSize))
            {
                Vector3 pos = positions[i] + new Vector3(0, 0, -stepSize / 2);
                Instantiate(wallTile, pos, Quaternion.identity, transform);
            }
            if (!positions.Contains(positions[i] + Vector3.right * stepSize))
            {
                Vector3 pos = positions[i] + new Vector3(stepSize / 2, 0, 0);
                Instantiate(wallTile, pos, Quaternion.Euler(0, 90, 0), transform);
            }
            if (!positions.Contains(positions[i] + Vector3.left * stepSize))
            {
                Vector3 pos = positions[i] + new Vector3(-stepSize / 2, 0, 0);
                Instantiate(wallTile, pos, Quaternion.Euler(0, 90, 0), transform);
            }
            //Add Floors and Ceilings
            if (!positions.Contains(positions[i] + Vector3.up * stepSize))
            {
                Instantiate(floorTile, positions[i] + (Vector3.up * (stepSize / 2)), Quaternion.identity, transform);
            }
            if (!positions.Contains(positions[i] + Vector3.down * stepSize))
            {
                Instantiate(floorTile, positions[i] + Vector3.down * (stepSize / 2), Quaternion.identity, transform);
            }
        }
    }

    Vector3 RandomRoomPosition(Vector3Int min, Vector3Int max)
    {
        int x = Random.Range(min.x, max.x);
        int y = (Random.Range(min.y, max.y) / (maxRoomSize.y + 1)) * (maxRoomSize.y + 1);
        int z = Random.Range(min.z, max.z);
        return new Vector3(x, y, z) * stepSize;
    }

    Vector3Int RandomRoomSize(Vector3Int min , Vector3Int max)
    {
        int x = Random.Range(min.x, max.x + 1);
        int y = Random.Range(min.y, max.y + 1);
        int z = Random.Range(min.z, max.z + 1);
        return new Vector3Int(x, y, z);
    }
}
