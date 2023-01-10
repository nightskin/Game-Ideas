using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject slitherPrefab;

    public float delay = 0.25f;
    public float spawnInterval = 60;
    public int spawnAmount = 10;

    float delayTimer;
    float spawnTimer;
    int spawnNumber;

    void Start()
    {
        delayTimer = 0;
        spawnTimer = 0;
        spawnNumber = 0;
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if(spawnTimer <= 0)
        {
            Spawn();
        }
    }

    void Spawn()
    {
        delayTimer -= Time.deltaTime;
        if (delayTimer <= 0 && spawnNumber != spawnAmount)
        {
            Instantiate(slitherPrefab, transform.position, Quaternion.identity, transform);
            delayTimer = delay;
            spawnNumber++;
            if(spawnNumber == spawnAmount)
            {
                spawnTimer = spawnInterval;
                spawnNumber = 0;
            }
        }
    }
}
