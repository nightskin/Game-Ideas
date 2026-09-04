using UnityEngine;

public class LevelMeshChunk : MonoBehaviour
{
    public LevelType type = LevelType.DUNGEON;
    public LevelStyle style = LevelStyle.CHUNKY;
    public MeshFilter meshFilter;
    public MeshRenderer renderer;
    public MeshCollider collider;
    public Vector3Int index;
    public LevelMeshGenerator generator;
    [HideInInspector] public float[] grid;

    public int chunkSize;
    public static int numThreads = 8;
    public static float chunkScale = 1;

    public virtual void Generate(){}
}
