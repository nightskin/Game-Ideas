using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrail : MonoBehaviour
{
    [SerializeField] Transform weaponTip;
    [SerializeField] Transform weaponBase;

    Vector3 prevTipPosition;
    Vector3 prevBasePosition;

    MeshFilter filter;
    Mesh mesh;

    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    void Start()
    {
        mesh = new Mesh();
        filter = GetComponent<MeshFilter>();
        filter.mesh = mesh;

    }

    


    void Update()
    {
        

        
        prevBasePosition = weaponBase.position;
        prevTipPosition = weaponTip.position;
    }
}
