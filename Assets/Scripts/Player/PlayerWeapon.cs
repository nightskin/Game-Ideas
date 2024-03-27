using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon: MonoBehaviour
{
    public BladeSystem bladeSystem;
    public GameObject impactEffectEnemy;
    public GameObject impactEffectSolid;
    
    public bool attacking = false;
    public bool defending = false;
    public float knockbackForce = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (attacking)
        {
            if (other.transform.tag == "Solid")
            {
                Instantiate(impactEffectSolid, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                bladeSystem.animator.SetTrigger("recoil");
                attacking = false;
            }
            else if (other.transform.tag == "CanBeDamaged")
            {
                Instantiate(impactEffectEnemy, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                if(other.attachedRigidbody)
                {
                    Vector3 forceDirection = (transform.root.right * bladeSystem.atkVector.x + transform.root.up * bladeSystem.atkVector.y);
                    other.attachedRigidbody.AddForce(forceDirection * knockbackForce);
                }

            }
        }
    }
}
