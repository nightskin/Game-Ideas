using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    Noise noise;
    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    Color[] colors;
    int[] triangles;

    [SerializeField] int tilesX = 500;
    [SerializeField] int tilesZ = 500;
    [SerializeField] float noiseScale = 0.1f;
    [SerializeField] float maxHeight = 100;
    [SerializeField] Gradient landGradient;
    [SerializeField] string seed = string.Empty;

    void Start()
    {
        if(seed == string.Empty) seed = Random.Range(int.MinValue, int.MaxValue).ToString();
        noise = new Noise(seed.GetHashCode());
        transform.position = new Vector3(-tilesX/2, 0, -tilesZ/2);
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        CreateTerrainGrid();
        UpdateMesh();
    }


    void CreateTerrainGrid()
    {
        vertices = new Vector3[(tilesX + 1) * (tilesZ + 1)];
        for(int i = 0, z = 0; z <= tilesZ; z++) 
        {
            for (int x = 0; x <= tilesX; x++)
            {
                float y = noise.Evaluate(new Vector3(x, 0, z) * noiseScale);
                vertices[i] = new Vector3(x, y * maxHeight, z);
                i++;
            }
        }
        
        triangles = new int[tilesX * tilesZ * 6];
        int vert = 0;
        int tri = 0;
        for(int z = 0; z < tilesZ; z++) 
        {
            for (int x = 0; x < tilesX; x++)
            {
                triangles[tri + 0] = vert + 0;
                triangles[tri + 1] = vert + tilesX + 1;
                triangles[tri + 2] = vert + 1;
                triangles[tri + 3] = vert + 1;
                triangles[tri + 4] = vert + tilesX + 1;
                triangles[tri + 5] = vert + tilesX + 2;

                vert++;
                tri += 6;
            }
            vert++;
        }

        uvs = new Vector2[vertices.Length];
        for (int i = 0, z = 0; z <= tilesZ; z++)
        {
            for (int x = 0; x <= tilesX; x++)
            {
                uvs[i] = new Vector2((float)x / tilesX, (float)z / tilesZ);
                i++;
            }
        }

        colors = new Color[vertices.Length];
        for (int i = 0, z = 0; z <= tilesZ; z++)
        {
            for (int x = 0; x <= tilesX; x++)
            {
                float y = Mathf.InverseLerp(-maxHeight, maxHeight, vertices[i].y);
                colors[i] = landGradient.Evaluate(y);
                i++;
            }
        }


    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
