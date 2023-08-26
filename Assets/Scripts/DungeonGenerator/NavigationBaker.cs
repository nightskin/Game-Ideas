using UnityEngine;
using UnityEngine.AI;

public class NavigationBaker : MonoBehaviour
{
    [SerializeField] DungeonGen level;
    [SerializeField] GameObject[] enemyTypes;
    [SerializeField] [Range(0, 100)] int spawnChance = 10;

    public NavMeshSurface surface;
    bool enemiesPlaced = false;
    bool navMeshMade = false;
    

    void Update()
    {
        if(!navMeshMade)
        {
            surface.BuildNavMesh();
            navMeshMade = true;
            if (!enemiesPlaced && enemyTypes.Length > 0) PlaceEnemies();
        }
    }

    void PlaceEnemies()
    {
        for(int f = 0; f < level.numberOfFloors; f++)
        {
            for (int x = 0; x < level.tilesX; x++)
            {
                for (int z = 0; z < level.tilesZ; z++)
                {
                    if (x < level.tilesX - 1 && z < level.tilesZ - 1)
                    {
                        Square square = new Square(level.map[x, z, f].position, level.tileSize);
                        string state = level.GetState(level.map[x, z, f].on, level.map[x, z + 1, f].on, level.map[x + 1, z, f].on, level.map[x + 1, z + 1, f].on);
                        if (state == "1111")
                        {
                            int outcome = level.random.Next(0, 101);
                            if (outcome <= spawnChance)
                            {
                                int enemyIndex = level.random.Next(0, enemyTypes.Length);
                                Instantiate(enemyTypes[enemyIndex], square.center, Quaternion.identity);
                            }
                        }
                    }
                }
            }
        }

        enemiesPlaced = true;
    }
}
