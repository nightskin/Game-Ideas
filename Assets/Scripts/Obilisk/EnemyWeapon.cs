using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public ObliliskAI owner;
    public GameObject impactEffectSolid;
    public bool attacking;

    private void OnTriggerEnter(Collider other)
    {
        if(attacking)
        {
            if(other.tag == "PlayerWeapon")
            {
                Instantiate(impactEffectSolid, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                owner.SwitchState(owner.EnemyStun);
            }
            if (other.transform.tag == "Wall")
            {
                Instantiate(impactEffectSolid, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if(attacking)
        {
            if (other.transform.tag == "PlayerWeapon")
            {
                Instantiate(impactEffectSolid, other.GetContact(0).point, Quaternion.identity);
                owner.SwitchState(owner.EnemyStun);
            }
            if(other.transform.tag == "Wall")
            {
                Instantiate(impactEffectSolid, other.GetContact(0).point, Quaternion.identity);
            }
        }
    }

}
