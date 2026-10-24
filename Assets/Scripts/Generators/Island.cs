using UnityEngine;

public class Island : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer renderer;
    public MeshCollider collider;
    [Min(8)] public Vector3Int sizeInVoxels; 
}
