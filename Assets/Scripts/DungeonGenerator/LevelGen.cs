using UnityEngine;

public class Space
{
    public Vector3 position;
    public bool on;
    public bool bottomHole;
    public bool topHole;
    public Space()
    {
        position = new Vector3();
        on = false;
        bottomHole = false;
        topHole = false;
    }
}

public class LevelGen : MonoBehaviour
{
    public Space[,] map = null;
    public int tilesX = 25;
    public int floor = 1;
    public int tilesZ = 25;
    public float stepSize = 10;
    public Vector3Int currentPos = Vector3Int.zero;


    void Awake()
    {
        map = new Space[tilesX,tilesZ];
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                map[x, z] = new Space();
            }
        }

        Gen1();

    }


    //2D Random Walker
    public void Gen1()
    {
        //Random Walker
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                int dir = Random.Range(1, 5);
                map[currentPos.x, currentPos.z].on = true;
                if (dir == 1)
                {
                    if( currentPos.x < tilesX - 1)
                    {
                        currentPos.x++;
                    }
                }
                if (dir == 2)
                {
                    if(currentPos.x > 0)
                    {
                        currentPos.x--;
                    }
                }
                if (dir == 3)
                {
                    if(currentPos.z < tilesZ - 1)
                    {
                        currentPos.z++;
                    }
                }
                if (dir == 4)
                {
                    if (currentPos.z > 0)
                    {
                        currentPos.z--;
                    }
                }
                map[x, z].position = new Vector3(x, 0, z);
            }
        }



    }



}
