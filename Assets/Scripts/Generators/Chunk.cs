using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Terrain))]
[RequireComponent (typeof(TerrainCollider))]
public class Chunk : MonoBehaviour
{
    [SerializeField] int numberOfTrees = 1000;
    [SerializeField] GameObject[] trees;
    [SerializeField] string seed = string.Empty;
    [SerializeField][Range(0, 1)] float noiseScale = 0.1f;
    [SerializeField][Range(0, 1)] float maxHeight = 0.1f;
    [SerializeField] Vector3 offset = Vector3.zero;
    [SerializeField] AnimationCurve slope;

    int terrainResX = 513;
    int terrainResZ = 513;

    Noise noise;
    Terrain terrain;
    Transform player;

    void Start()
    {
        //Initialize Land
        terrain = GetComponent<Terrain>();
        noise = new Noise(seed.GetHashCode());
        Random.InitState(seed.GetHashCode());

        //Draw Chunk
        Draw();
        PlaceDetails();

        //Place Player
        player = GameObject.FindGameObjectWithTag("Player").transform;
        float y = terrain.terrainData.GetHeight(terrainResX / 2, terrainResZ / 2);
        player.transform.position = new Vector3(500, y, 500);
    }

    void OnValidate()
    {
        terrain = GetComponent<Terrain>();
        noise = new Noise(seed.GetHashCode());
        Random.InitState(seed.GetHashCode());
        Draw();
    }

    void Draw()
    {
        var data = new float[terrainResX, terrainResZ];
        for (int z = 0; z < terrainResZ; z++)
        {
            for (int x = 0; x < terrainResX; x++)
            {
                data[x, z] = EvaluateHeight(new Vector3(x, 0, z) + offset);
            }
        }
        terrain.terrainData.SetHeights(0, 0, data);
    }

    void PlaceDetails()
    {
        //PlaceTrees
        for(int tree = 0; tree < numberOfTrees; tree++) 
        {
            int i = Random.Range(0, trees.Length);
            var detail = Instantiate(trees[i], RandomPos(), trees[i].transform.rotation, transform);
        }
    }

    float EvaluateHeight(Vector3 position)
    {
        float h = slope.Evaluate(noise.Evaluate(position * noiseScale)) * maxHeight;
        return h;
    }
    
    Vector3 RandomPos()
    {
        float x = Random.Range(0, 1000) + transform.position.x;
        float z = Random.Range(0, 1000) + transform.position.z;
        float y = terrain.SampleHeight(new Vector3(x, 0, z));

        return new Vector3(x, y, z);
    }
}
