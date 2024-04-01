using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    [SerializeField] string seed = string.Empty;

    [SerializeField] Transform player;
    [SerializeField] GameObject buildingPrefab;
    List<GameObject> buildingObjects = new List<GameObject>();

    [SerializeField] int numberOfBuildings = 10;
    [SerializeField] float spacing = 50;
    [SerializeField] float maxBuildingSize = 1;
    [SerializeField] float minBuildingSize = 1;


    void Awake()
    {
        if (seed == string.Empty) seed = DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());

        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;
        for(int x = 0; x < numberOfBuildings; x++)
        {
            for(int z = 0;  z < numberOfBuildings; z++) 
            {
                int makeBuilding = Mathf.RoundToInt(UnityEngine.Random.value);
                if(makeBuilding == 1)
                {
                    var building = Instantiate(buildingPrefab, new Vector3(x * spacing, 0, z * spacing), Quaternion.identity, transform);
                    buildingObjects.Add(building);
                    float width = UnityEngine.Random.Range(minBuildingSize, maxBuildingSize);
                    float depth = UnityEngine.Random.Range(minBuildingSize, maxBuildingSize);
                    building.transform.localScale = new Vector3(width, 1, depth);
                }
            }
        }
    }

    void Start()
    {
        if(transform.Find("Ground")) transform.Find("Ground").localPosition = new Vector3(maxBuildingSize * spacing * 2, 0, maxBuildingSize * spacing * 2);
        int i = buildingObjects.Count / 2;
        GameObject b = buildingObjects[i];
        b.GetComponent<BuildingGenerator>().SpawnPlayer();
    }
}
