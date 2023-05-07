using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulsate : MonoBehaviour
{
    float normal = 0.005f;
    
    void Update()
    {
        normal = Mathf.PingPong(Time.time * 0.25f, 0.08f);
        GetComponent<MeshRenderer>().material.SetFloat("_Parallax", normal);
    }
}
