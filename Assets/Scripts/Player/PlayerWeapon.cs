using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon: MonoBehaviour
{
    public FirstPersonPlayer player;
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
                other.transform.GetComponentInParent<EnemyHealth>().TakeDamage();
            }
            if (other.transform.tag == "EnemyWeapon")
            {
                Instantiate(impactEffectSolid, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                meleeSystem.animator.SetTrigger("recoil");
            }

        }
    }

}
