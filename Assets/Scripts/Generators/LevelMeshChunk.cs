using UnityEngine;

public class LevelMeshChunk : MonoBehaviour
{
    public enum LevelType
    {
        DUNGEON,
        CAVES,
        TERRAIN,
    }
    public LevelType levelType = LevelType.DUNGEON;
    public enum ArtStyle
    {
        BLOCKY,
        CHUNKY,
        SMOOTH,
    }
    public ArtStyle style = ArtStyle.CHUNKY;
    public MeshFilter meshFilter;
    public MeshRenderer renderer;
    public MeshCollider collider;
    public Vector3Int index;
    public LevelMeshGenerator generator;
    public float[] grid = new float[chunkSize * chunkSize * chunkSize];

    public static int chunkSize = 16;
    public static int numThreads = 8;
    public static float chunkScale = 3;

    public virtual void Generate(){}
}
