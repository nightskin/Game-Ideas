using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    [SerializeField] FirstPersonPlayer player;
    [SerializeField] MeleeSystem meleeSystem;
    public bool attacking;
    public int damage = 1;
    public float knockback = 100;
    


    private void OnTriggerEnter(Collider other)
    {
        if(attacking)
        {
           if(other.attachedRigidbody)
           {
                //other.attachedRigidbody.AddForceAtPosition()
           }
        }

    }

}
