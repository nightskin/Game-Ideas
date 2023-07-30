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
                Instantiate(impactEffectEnemy, other.ClosestPoint(transform.position), Quaternion.identity);
            }
            if (other.transform.tag == "EnemyWeapon")
            {
                Instantiate(impactEffectSolid, other.ClosestPoint(transform.position), Quaternion.identity);
                meleeSystem.animator.SetTrigger("recoil");
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
            }
            if (other.transform.tag == "EnemyWeapon")
            {
                Instantiate(impactEffectSolid, other.GetContact(0).point, Quaternion.identity);
                meleeSystem.animator.SetTrigger("recoil");
            }

        }
    }

}
