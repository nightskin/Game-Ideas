using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    public int minNumberOfFloors = 10;
    public int maxNumberOfFloors = 15;
    public int spacingBetweenFloors = 11;

    int numberOfFloors;
    [SerializeField] GameObject[] stories;
    [SerializeField] GameObject[] roofs;

    void Awake()
    {
        numberOfFloors = Random.Range(minNumberOfFloors, maxNumberOfFloors + 1);
        for(int i = 0; i < numberOfFloors; i++) 
        {
            if(i < numberOfFloors - 1)
            {
                int r = Random.Range(0, stories.Length);
                var b = Instantiate(stories[r], Vector3.zero, Quaternion.Euler(0, 0, 0), transform);
                b.transform.localPosition = Vector3.up * i * spacingBetweenFloors;
            }
            else
            {
                int r = Random.Range(0, roofs.Length);
                var b = Instantiate(roofs[r], Vector3.zero, Quaternion.Euler(0, 0, 0), transform);
                b.transform.gameObject.name = "roof";
                b.transform.localPosition = Vector3.up * i * spacingBetweenFloors;
            }
        }
    }

    public void SpawnPlayer()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        player.transform.position = transform.Find("roof").transform.position + (Vector3.up * (spacingBetweenFloors + 2));
    }
}
