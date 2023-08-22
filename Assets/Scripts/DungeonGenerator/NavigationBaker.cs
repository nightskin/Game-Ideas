using UnityEngine;
using UnityEngine.AI;

public class NavigationBaker : MonoBehaviour
{
    [SerializeField] DungeonGen level;
    [SerializeField] GameObject enemyPrafab;
    [SerializeField] [Range(1, 100)] int spawnChance = 10;

    public NavMeshSurface surface;
    bool enemiesPlaced = false;
    bool navMeshMade = false;
    

    void Update()
    {
        if(!navMeshMade)
        {
            surface.BuildNavMesh();
            navMeshMade = true;
            if (!enemiesPlaced && enemyPrafab) PlaceEnemies();
        }
    }

    void PlaceEnemies()
    {
        for (int x = 0; x < level.tilesX; x++)
        {
            for (int z = 0; z < level.tilesZ; z++)
            {
                if (x < level.tilesX - 1 && z < level.tilesZ - 1)
                {
                    Square square = new Square(level.map2d[x, z].position, level.tileSize);
                    string state = level.GetState(level.map2d[x, z].on, level.map2d[x, z + 1].on, level.map2d[x + 1, z].on, level.map2d[x + 1, z + 1].on);
                    if (state == "1111")
                    {
                        int g = level.random.Next(0, 101);
                        if (g <= spawnChance) 
                        {
                            Instantiate(enemyPrafab, square.center, Quaternion.identity);
                        }
                    }
                }
            }
        }
        enemiesPlaced = true;
    }
}
