using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon: MonoBehaviour
{
    public Color trailColor = Color.white;
    public BladeSystem bladeSystem;
    public ParticleSystem trail;
    public GameObject impactEffectEnemy;
    public GameObject impactEffectSolid;
    
    public bool attacking = false;
    public bool defending = false;
    public float knockbackForce = 10;

    private void Start()
    {
        bladeSystem = transform.root.GetComponent<BladeSystem>();
        if(transform.Find("Trail")) trail = transform.Find("Trail").GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (attacking)
        {
            if (other.transform.tag == "Solid" || other.tag == "EnemyWeapon")
            {
                if(impactEffectSolid) Instantiate(impactEffectSolid, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                bladeSystem.animator.SetTrigger("recoil");
                attacking = false;
            }
            else if (other.transform.tag == "CanBeDamaged")
            {
                if(impactEffectEnemy) Instantiate(impactEffectEnemy, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                if (other.transform.root.GetComponent<EnemyHealth>())
                {
                    EnemyHealth health = other.transform.root.GetComponent<EnemyHealth>();
                    health.TakeDamage();
                    if(health.IsDead())
                    {
                        health.SetRagDoll(true);
                    }
                }

                if (other.attachedRigidbody)
                {
                    Vector3 forceDirection = (transform.root.right * bladeSystem.atkVector.x + transform.root.up * bladeSystem.atkVector.y);
                    other.attachedRigidbody.AddForce(forceDirection * knockbackForce);
                }

            }
        }
    }
}
