using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon: MonoBehaviour
{
    public MeleeSystem meleeSystem;
    public GameObject impactEffectEnemy;
    public GameObject impactEffectSolid;
    
    public bool attacking;
    public int damage = 1;
    public float knockbackForce = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (attacking)
        {
            if (other.transform.tag == "Enemy")
            {
                Instantiate(impactEffectEnemy, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                if (other.GetComponent<EnemyHealth>()) other.GetComponent<EnemyHealth>().TakeDamage();
                if (other.attachedRigidbody) other.attachedRigidbody.AddForce(-transform.right * knockbackForce);
                attacking = false;
            }
            else if (other.transform.tag == "EnemyWeapon")
            {
                Instantiate(impactEffectSolid, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                meleeSystem.animator.SetTrigger("recoil");
                attacking = false;
            }

        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (attacking)
        {
            if (other.transform.tag == "Enemy")
            {
                Instantiate(impactEffectEnemy, other.GetContact(0).point, Quaternion.identity);
                if (other.transform.GetComponent<EnemyHealth>()) other.transform.GetComponent<EnemyHealth>().TakeDamage();
                if (other.rigidbody) other.rigidbody.AddForce(-transform.right * knockbackForce);
                attacking = false;
            }
            else if (other.transform.tag == "EnemyWeapon")
            {
                Instantiate(impactEffectSolid, other.GetContact(0).point, Quaternion.identity);
                meleeSystem.animator.SetTrigger("recoil");
                attacking = false;
            }

        }
    }

}
