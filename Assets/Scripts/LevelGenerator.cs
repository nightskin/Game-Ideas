using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    int buffer = 0;

    float normal = 0.005f;

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateRoom(Vector3.zero, 15);
        UpdateMesh();
    }

    void Update()
    {
        normal = Mathf.PingPong(Time.time * 0.25f, 0.08f);
        GetComponent<MeshRenderer>().material.SetFloat("_Parallax", normal);
    }

    public void CreateQuad(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight, float scale = 1)
    {
        vertices.Add(topLeft * scale);
        vertices.Add(topRight * scale);
        vertices.Add(bottomLeft * scale);
        vertices.Add(bottomRight * scale);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));

        triangles.Add(0 + buffer);
        triangles.Add(1 + buffer);
        triangles.Add(2 + buffer);
        triangles.Add(1 + buffer);
        triangles.Add(3 + buffer);
        triangles.Add(2 + buffer);

        buffer += 4;
    }

    public void CreateBox(Vector3 position, float scale = 1)
    {
        //Top
        CreateQuad(new Vector3(1, -1, 1) + position, new Vector3(-1, -1, 1) + position, new Vector3(1, -1, -1) + position, new Vector3(-1, -1, -1) + position, scale);
        //Botom
        CreateQuad(new Vector3(-1, 1, 1) + position, new Vector3(1, 1, 1) + position, new Vector3(-1, 1, -1) + position, new Vector3(1, 1, -1) + position, scale);

        //Wall Back
        CreateQuad(new Vector3(-1, 1, -1) + position, new Vector3(1, 1, -1) + position, new Vector3(-1, -1, -1) + position, new Vector3(1, -1, -1) + position, scale);
        //Wall Front
        CreateQuad(new Vector3(1, 1, 1) + position, new Vector3(-1, 1, 1) + position, new Vector3(1, -1, 1) + position, new Vector3(-1, -1, 1) + position, scale);

        //Wall Right
        CreateQuad(new Vector3(1, 1, -1) + position, new Vector3(1, 1, 1) + position, new Vector3(1, -1, -1) + position, new Vector3(1, -1, 1) + position, scale);
        //Wall Left
        CreateQuad(new Vector3(-1, 1, 1) + position, new Vector3(-1, 1, -1) + position, new Vector3(-1, -1, 1) + position, new Vector3(-1, -1, -1) + position, scale);

    }

    public void CreateRoom(Vector3 position, float scale = 1)
    {
        //Top
        CreateQuad(new Vector3(-1, 0, 1) + position, new Vector3(1, 0, 1) + position, new Vector3(-1, 0, -1) + position, new Vector3(1, 0, -1) + position, scale);
        //Botom
        CreateQuad(new Vector3(1, 1, 1) + position, new Vector3(-1, 1, 1) + position, new Vector3(1, 1, -1) + position, new Vector3(-1, 1, -1) + position, scale);

        //Wall Fromt
        CreateQuad(new Vector3(1, 1, -1) + position, new Vector3(-1, 1, -1) + position, new Vector3(1, 0, -1) + position, new Vector3(-1, 0, -1) + position, scale);
        //Wall Back
        CreateQuad(new Vector3(-1, 1, 1) + position, new Vector3(1, 1, 1) + position, new Vector3(-1, 0, 1) + position, new Vector3(1, 0, 1) + position, scale);

        //Wall Right
        CreateQuad(new Vector3(1, 1, 1) + position, new Vector3(1, 1, -1) + position, new Vector3(1, 0, 1) + position, new Vector3(1, 0, -1) + position, scale);
        //Wall Left
        CreateQuad(new Vector3(-1, 1, -1) + position, new Vector3(-1, 1, 1) + position, new Vector3(-1, 0, -1) + position, new Vector3(-1, 0, 1) + position, scale);
    }
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
