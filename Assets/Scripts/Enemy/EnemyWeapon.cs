using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public EnemyStateMachine enemy;
    public bool attacking = false;

    private void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "PlayerWeapon" && attacking)
        {
            Debug.Log("Clash");

            enemy.SwitchState(enemy.enemyStun);
        }
        else if (other.transform.tag == "Player" && attacking)
        {
            other.transform.GetComponent<FirstPersonPlayer>().stun = true;
            other.transform.GetComponent<FirstPersonPlayer>().knockback = new Vector3(transform.forward.x, 0, transform.forward.y);
        }
    }

}
