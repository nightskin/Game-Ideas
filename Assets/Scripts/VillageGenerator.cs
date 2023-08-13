using System;
using UnityEngine;

public class VillageGenerator : MonoBehaviour
{
    [SerializeField] GameObject[] buildings;
    [SerializeField] float offset = 20;
    [SerializeField] int maxX = 25;
    [SerializeField] int maxZ = 25;

    [SerializeField] bool randomSeed = false;
    [SerializeField] string seed = "1";

    System.Random random;


    void Start()
    {
        if (randomSeed) seed = System.DateTime.Now.ToString();

        random = new System.Random(seed.GetHashCode());

        for(int x = 0; x < maxX; x++)
        {
            for(int z = 0; z < maxZ; z++)
            {
                int makeBuilding = random.Next(0, 2);
                if(makeBuilding == 1)
                {
                    int buildingIndex = random.Next(0, buildings.Length);
                    Instantiate(buildings[buildingIndex], new Vector3(x - maxX / 2, 0, z - maxZ / 2) * offset, Quaternion.identity);
                }
            }
        }

        Destroy(gameObject);

    }

    
}
