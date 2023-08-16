using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public bool attacking = false;

    private void OnCollisionEnter(Collision other)
    {
        if(attacking)
        {
            if(other.gameObject.tag == "Player")
            {

            }
            else if(other.gameObject.tag == "PlayerWeapon")
            {

            }
        }
    }

}
