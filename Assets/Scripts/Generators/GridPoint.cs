using UnityEngine;

public class GridPoint
{
    public Vector3 position;
    public float value = 0;
    
    public GridPoint()
    {
        position = Vector3.zero;
    }

    public static int GetState(GridPoint[] points, float isoLevel)
    {
        int state = 0;
        if (points[0].value > isoLevel) state |= 1;
        if (points[1].value > isoLevel) state |= 2;
        if (points[2].value > isoLevel) state |= 4;
        if (points[3].value > isoLevel) state |= 8;
        if (points[4].value > isoLevel) state |= 16;
        if (points[5].value > isoLevel) state |= 32;
        if (points[6].value > isoLevel) state |= 64;
        if (points[7].value > isoLevel) state |= 128;
        return state;
    }

    public static Vector3 MidPoint(GridPoint p1, GridPoint p2)
    {
        return p1.position + p2.position / 2;
    }

    public static Vector3 LerpPoint(GridPoint p1,  GridPoint p2, float isoLevel)
    {
        float amount = (isoLevel - p1.value) / (p2.value - p1.value);
        return Vector3.Lerp(p1.position, p2.position, amount);
    }

}