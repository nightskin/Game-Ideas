using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashScript : MonoBehaviour
{
    [SerializeField] Vector3 rotationAxis = Vector3.zero;
    [SerializeField] float rotationSpeed = 10;

    [SerializeField] Vector3 checkExtents = Vector3.one;


    void Start()
    {
        rotationAxis = rotationAxis.normalized;
    }
    
    void Update()
    {
        
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawWireCube(transform.position - transform.up * 1.3f, checkExtents);
    }
}
