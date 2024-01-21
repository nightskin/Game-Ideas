using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon: MonoBehaviour
{
    public MeleeSystem meleeSystem;
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
                if (other.transform.root.GetComponent<EnemyHealth>()) other.transform.root.GetComponent<EnemyHealth>().TakeDamage();
                Vector3 atkDirection = meleeSystem.transform.right * meleeSystem.atkVector.x + Vector3.up * meleeSystem.atkVector.y;
                if (other.attachedRigidbody) other.attachedRigidbody.AddForce(atkDirection * knockbackForce);
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


}
