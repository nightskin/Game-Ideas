using UnityEngine;
using UnityEngine.AI;

public class NavigationBaker : MonoBehaviour
{
    [SerializeField] DungeonGen level;
    [SerializeField] GameObject[] enemyTypes;
    [SerializeField] [Range(0, 100)] int spawnChance = 10;

    public NavMeshSurface surface;
    public void PlaceEnemies()
    {
        for (int x = 0; x < level.tilesX; x++)
        {
            for (int z = 0; z < level.tilesZ; z++)
            {
                if (x < level.tilesX - 1 && z < level.tilesZ - 1)
                {
                    Vector3[] corners =
                    {
                        level.map[x,z].position,
                        level.map[x,z+1].position,
                        level.map[x + 1, z].position,
                        level.map[x + 1, z + 1].position,
                    };

                    Vector3[] midPoints = 
                    {
                        level.map[x,z].position + new Vector3(0, 0, level.tileSize/2),
                        level.map[x,z].position + new Vector3(level.tileSize, 0, level.tileSize/2),
                        level.map[x,z].position + new Vector3(level.tileSize/2, 0, level.tileSize),
                        level.map[x,z].position + new Vector3(level.tileSize/2, 0, 0),
                    };

                    Square square = new Square(level.map[x, z].position, level.tileSize, corners, midPoints);
                    string state = level.GetState(level.map[x, z].on, level.map[x, z + 1].on, level.map[x + 1, z].on, level.map[x + 1, z + 1].on);
                    if (state == "1111")
                    {
                        int outcome = Random.Range(0, 100 + 1);
                        if (outcome < spawnChance && enemyTypes.Length > 0)
                        {
                            int enemyIndex = Random.Range(0, enemyTypes.Length);
                            Instantiate(enemyTypes[enemyIndex], square.center, Quaternion.identity);
                        }
                    }
                }
            }
        }

    }
}
