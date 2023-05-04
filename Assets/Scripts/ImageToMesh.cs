using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ImageToMesh : MonoBehaviour
{
    public float heightScale = 10;
    public bool invertHeight = false;
    public Texture2D heightTexture;

    Vector3[] verts;
    Color[] colors;
    int[] tris;
    Mesh currentMesh;

    void Start()
    {
        currentMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = currentMesh;
        if (heightTexture.width > 255 || heightTexture.height > 255)
        {
            currentMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }
        RenderImage();
        UpdateMesh(currentMesh);
    }

    void RenderImage()
    {

        verts = new Vector3[(heightTexture.width + 1) * (heightTexture.height + 1)];
        for (int x = 0, i = 0; x <= heightTexture.width; x++)
        {
            for (int z = 0; z <= heightTexture.height; z++)
            {
                float brightness = heightTexture.GetPixel(x, z).grayscale;
                float r = heightTexture.GetPixel(x, z).r;
                float g = heightTexture.GetPixel(x, z).g;
                float b = heightTexture.GetPixel(x, z).b;

                float y;
                if(invertHeight)
                {
                    y = -brightness * heightScale;
                }
                else
                {
                    y = brightness * heightScale;
                }

                verts[i] = new Vector3(x - heightTexture.width / 2, y, z - heightTexture.height / 2);
                i++;
            }
        }

        int v = 0;
        int t = 0;
        tris = new int[heightTexture.width * heightTexture.height * 6];
        for (int x = 0; x < heightTexture.width; x++)
        {
            for (int z = 0; z < heightTexture.height; z++)
            {
                
                tris[t + 0] = v + 1;
                tris[t + 1] = v + heightTexture.width + 1;
                tris[t + 2] = v + 0;
                tris[t + 3] = v + heightTexture.width + 2;
                tris[t + 4] = v + heightTexture.width + 1;
                tris[t + 5] = v + 1;
                v++;
                t += 6;
            }
            v++;
        }


        colors = new Color[verts.Length];
        for (int x = 0, i = 0; x <= heightTexture.width; x++)
        {
            for (int z = 0; z <= heightTexture.height; z++)
            {
                colors[i] = heightTexture.GetPixel(x, z);
                i++;
            }
        }
        


    }

    void UpdateMesh(Mesh mesh)
    {
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void OnValidate()
    {
        if(heightTexture != null)
        {
            var mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            if(heightTexture.width > 255 || heightTexture.height > 255)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
 
            RenderImage();
            mesh.Clear();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.colors = colors;
            mesh.RecalculateNormals();
        }

    }

}
