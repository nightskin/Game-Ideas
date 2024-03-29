using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] GameObject[] buildingPrefabs;
    GameObject[,] buildingObjects;

    [SerializeField] int numberOfBuildings = 10;
    [SerializeField] float buidlingSpacing = 100;
    [SerializeField] float maxBuildingHeight = 100;
    [SerializeField] float minBuildingHeight = 25;


    void Start()
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;
        buildingObjects = new GameObject[numberOfBuildings,numberOfBuildings];
        for(int x = 0; x < numberOfBuildings; x++)
        {
            for(int z = 0;  z < numberOfBuildings; z++) 
            {
                int index = Random.Range(0, buildingPrefabs.Length);
                var building = Instantiate(buildingPrefabs[index], new Vector3(x * buidlingSpacing, 0, z * buidlingSpacing), Quaternion.identity, transform);
                buildingObjects[x,z] = building;
                float height = Random.Range(minBuildingHeight, maxBuildingHeight);
                building.transform.localScale = new Vector3(1, height, 1);
            }
        }

        int randomIndexX = Random.Range(0, numberOfBuildings);
        int randomIndexY = Random.Range(0, numberOfBuildings);
        GameObject randomBuiding = buildingObjects[randomIndexX,randomIndexY];

        player.transform.position = new Vector3(numberOfBuildings / 2 * buidlingSpacing, 100 * randomBuiding.transform.localScale.y , numberOfBuildings / 2 * buidlingSpacing);
    }
}
