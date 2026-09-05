using UnityEngine;



public static class Util
{
    public static Vector3 RandomPositionInBox(float minx, float maxx, float miny, float maxy, float minz, float maxz)
    {
        float x = UnityEngine.Random.Range(minx,maxx);
        float y = UnityEngine.Random.Range(miny,maxy);
        float z = UnityEngine.Random.Range(minz,maxz);
        return new Vector3(x,y,z);
    }

    public static float Remap(float value, float oldMin, float oldMax, float newMin, float newMax)
    {
        return (((value - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
    }

    public static float InvertRange(float value, float min, float max)
    {
        return max - value + min;
    }
}
