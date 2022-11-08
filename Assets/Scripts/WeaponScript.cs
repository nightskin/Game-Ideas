using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    public bool inUse;
    public int damage = 1;
    public float knockback = 100;

    private void OnTriggerEnter(Collider other)
    {
        if(inUse)
        {
            if (other.gameObject.name != "Player")
            {
                if (other.attachedRigidbody)
                {
                    other.attachedRigidbody.AddForceAtPosition(-transform.up * knockback, other.ClosestPoint(transform.position));
                }
            }
        }

    }

}
