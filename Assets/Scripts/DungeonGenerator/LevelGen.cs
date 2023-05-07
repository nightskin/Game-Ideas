using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Space
{
    public float x;
    public float z;
    public float y;
    public bool on;
    public Space(float xPos, float zPos)
    {
        x = xPos;
        z = zPos;
        y = 0;
        on = false;
    }
    public Space (float xpos, float yPos, float zPos)
    {
        x = xpos;
        y = yPos;
        z = zPos;
        on = false;
    }
}

public class LevelGen : MonoBehaviour
{
    public Space[,] map = null;
    public int tilesX = 25;
    public int tilesZ = 25;
    public float stepSize = 10;
    Vector3Int currentPos;

    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        map = new Space[tilesX, tilesZ];
        currentPos = Vector3Int.zero;
        //Create Map
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                map[x, z] = new Space(x, z);
            }
        }

        Gen1();
        UpdateMesh();

    }


    void CreateQuad(Vector3 bottomLeft, Vector3 topLeft, Vector3 bottomRight, Vector3 topRight, Vector3 offset)
    {
        verts.Add(bottomLeft + offset);
        verts.Add(topLeft + offset);
        verts.Add(bottomRight + offset);
        verts.Add(topRight + offset);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));

        tris.Add(0 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        tris.Add(1 + buffer);
        tris.Add(3 + buffer);
        tris.Add(2 + buffer);

        buffer += 4;
    }

    void CreateQuad(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        verts.Add(Quaternion.Euler(rotation) * new Vector3(-1 * scale.x, -1 * scale.y, -1 * scale.z) + position);
        verts.Add(Quaternion.Euler(rotation) * new Vector3(-1 * scale.x, -1 * scale.y, 1 * scale.z) + position);
        verts.Add(Quaternion.Euler(rotation) * new Vector3(1 * scale.x, -1 * scale.y, -1 * scale.z) + position);
        verts.Add(Quaternion.Euler(rotation) * new Vector3(1 * scale.x, -1 * scale.y, 1 * scale.z) + position);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));


        tris.Add(0 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);

        tris.Add(1 + buffer);
        tris.Add(3 + buffer);
        tris.Add(2 + buffer);

        buffer += 4;
    }

    public void CreateWalls(Vector3 offset, Vector3 size, bool front = false, bool back = false, bool left = false, bool right = false, bool up = false, bool down = false)
    {
        //Left
        if (left)
            CreateQuad(new Vector3(-1 * size.x, -1 * size.y, -1 * size.z), new Vector3(-1 * size.x, 1 * size.y, -1 * size.z), new Vector3(-1 * size.x, -1 * size.y, 1 * size.z), new Vector3(-1 * size.x, 1 * size.y, 1 * size.z), offset);

        //Right
        if (right)
            CreateQuad(new Vector3(1 * size.x, 1 * size.y, -1 * size.z), new Vector3(1 * size.x, -1 * size.y, -1 * size.z), new Vector3(1 * size.x, 1 * size.y, 1 * size.z), new Vector3(1 * size.x, -1 * size.y, 1 * size.z), offset);

        //Back
        if (back)
            CreateQuad(new Vector3(-1 * size.x, 1 * size.y, -1 * size.z), new Vector3(-1 * size.x, -1 * size.y, -1 * size.z), new Vector3(1 * size.x, 1 * size.y, -1 * size.z), new Vector3(1 * size.x, -1 * size.y, -1 * size.z), offset);

        //Front
        if (front)
            CreateQuad(new Vector3(-1 * size.x, -1 * size.y, 1 * size.z), new Vector3(-1 * size.x, 1 * size.y, 1 * size.z), new Vector3(1 * size.x, -1 * size.y, 1 * size.z), new Vector3(1 * size.x, 1 * size.y, 1 * size.z), offset);

        //Up
        if (up)
            CreateQuad(new Vector3(-1 * size.x, 1 * size.y, 1 * size.z), new Vector3(-1 * size.x, 1 * size.y, -1 * size.z), new Vector3(1 * size.x, 1 * size.y, 1 * size.z), new Vector3(1 * size.x, 1 * size.y, -1 * size.z), offset);

        if (down)
            CreateQuad(new Vector3(-1 * size.x, -1 * size.y, -1 * size.z), new Vector3(-1 * size.x, -1 * size.y, 1 * size.z), new Vector3(1 * size.x, -1 * size.y, -1 * size.z), new Vector3(1 * size.x, -1 * size.y, 1 * size.z), offset);

    }




    //2D Random Walker
    public void Gen1()
    {
        //Random Walker
        for (int x = 0; x < tilesX; x++)
        {
            for (int z = 0; z < tilesZ; z++)
            {
                int dir = Random.Range(0, 5);
                if (map[currentPos.x, currentPos.z].on == false)
                {
                    CreateWalls((Vector3)currentPos * stepSize, Vector3.one * stepSize / 2, false, false, false, false, false, true);
                    map[currentPos.x, currentPos.z].on = true;
                }
                if (dir == 1)
                {
                    if(currentPos.x < tilesX - 1)
                    {
                        currentPos.x++;
                    }
                
                }
                else if (dir == 2)
                {
                    if(currentPos.x > 0)
                    {
                        currentPos.x--;
                    }
                
                }
                else if (dir == 3)
                {
                    if(currentPos.z < tilesZ - 1)
                    {
                        currentPos.z++;
                    }
                
                }
                else if (dir == 4)
                {
                    if (currentPos.z > 0)
                    {
                        currentPos.z--;
                    }
                }
            }
        }
    }



    public void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }


}
