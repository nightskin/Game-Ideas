using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshMonsterWeapon : MonoBehaviour
{
    [SerializeField] GameObject impactEffectSolid;
    [SerializeField] FleshMonsterAI ai;


    private void OnTriggerEnter(Collider other)
    {
        if(ai.attacking)
        {
            if(other.gameObject.tag == "Player")
            {

            }
            else if(other.gameObject.tag == "PlayerWeapon")
            {
                Instantiate(impactEffectSolid, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                ai.SwitchState(ai.stunState);
            }
        }
    }



}
