using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer meshRenderer;
    MeshCollider collider;
    public bool attacking = false;



    private void Start()
    {
        collider = GetComponent<MeshCollider>();
    }

    private void FixedUpdate()
    {
        UpdateCollider();
    }

    public void UpdateCollider()
    {
        Mesh colliderMesh = new Mesh();
        meshRenderer.BakeMesh(colliderMesh);
        collider.sharedMesh = colliderMesh;

    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

}
