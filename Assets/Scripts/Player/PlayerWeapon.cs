using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon: MonoBehaviour
{
    public BladeSystem meleeSystem;
    public GameObject impactEffectEnemy;
    public GameObject impactEffectSolid;
    
    public bool attacking;
    public float knockbackForce = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (attacking)
        {
            if (other.transform.tag == "Enemy")
            {
                Instantiate(impactEffectEnemy, other.ClosestPointOnBounds(transform.position), Quaternion.identity);

            }
            else if (other.transform.tag == "Solid")
            {
                Instantiate(impactEffectSolid, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                meleeSystem.animator.SetTrigger("recoil");
                attacking = false;
            }

        }
    }


}
