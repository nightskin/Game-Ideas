using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollision : MonoBehaviour
{
    public float power = 500;
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Moveable")
        {
            
        }

    }
}
