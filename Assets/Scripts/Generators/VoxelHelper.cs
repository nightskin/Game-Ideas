using UnityEngine;

public static class VoxelHelper
{    
    public static int Index3DToIndex(Vector3Int index3d, int chunkSize)
    {
        return index3d.x + chunkSize * (index3d.y + chunkSize * index3d.z);
        //return (index3d.x) + (index3d.y * chunkSize) + (index3d.z * (chunkSize * chunkSize));
    }

    public static Vector3Int IndexToIndex3D(int index, int chunkSize)
    {
        int x = index % chunkSize;
        int y = (index / chunkSize) % chunkSize;
        int z = ((index /chunkSize)/ chunkSize) % chunkSize;
        return new Vector3Int(x,y,z); 
    }

    public static int GetState(float[] values, float isoLevel)
    {
        int state = 0;
        if (values[0] > isoLevel) state |= 1;
        if (values[1] > isoLevel) state |= 2;
        if (values[2] > isoLevel) state |= 4;
        if (values[3] > isoLevel) state |= 8;
        if (values[4] > isoLevel) state |= 16;
        if (values[5] > isoLevel) state |= 32;
        if (values[6] > isoLevel) state |= 64;
        if (values[7] > isoLevel) state |= 128;
        return state;
    }

    public static Vector3 LerpPoint(float v1, float v2, Vector3 pos1, Vector3 pos2, float isoLevel)
    {
        float amount = (isoLevel - v1) / (v2 - v1);
        return Vector3.Lerp(pos1, pos2, amount);
    }

    public static Vector3 MidPoint(Vector3 pos1, Vector3 pos2)
    {
        return pos1 + pos2 / 2;
    }

    public static Vector2[] GetUVs(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 s1 = b - a;
        Vector3 s2 = c - a;
        Vector3 norm = Vector3.Cross(s1, s2).normalized; // the normal

        norm.x = Mathf.Abs(norm.x);
        norm.y = Mathf.Abs(norm.y);
        norm.z = Mathf.Abs(norm.z);

        Vector2[] uvs = new Vector2[3];
        if (norm.x >= norm.z && norm.x >= norm.y) // x plane
        {
            uvs[0] = new Vector2(a.z, a.y);
            uvs[1] = new Vector2(b.z, b.y);
            uvs[2] = new Vector2(c.z, c.y);
        }
        else if (norm.z >= norm.x && norm.z >= norm.y) // z plane
        {
            uvs[0] = new Vector2(a.x, a.y);
            uvs[1] = new Vector2(b.x, b.y);
            uvs[2] = new Vector2(c.x, c.y);
        }
        else if (norm.y >= norm.x && norm.y >= norm.z) // y plane
        {
            uvs[0] = new Vector2(a.x, a.z);
            uvs[1] = new Vector2(b.x, b.z);
            uvs[2] = new Vector2(c.x, c.z);
        }

        return uvs;
    }

}