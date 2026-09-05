using UnityEngine;

public class LevelMeshChunk : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer renderer;
    public MeshCollider collider;
    public Vector3Int index;
    public LevelMeshGenerator generator;
    [HideInInspector] public float[] grid;

    public int chunkSize;
    public static int numThreads = 8;
    public static float chunkScale = 2.5f;

    public virtual void Generate(){}
}
