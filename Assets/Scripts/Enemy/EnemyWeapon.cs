using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public EnemyStateMachine enemy;
    public bool attacking = false;


    private void Start()
    {
        enemy = transform.parent.parent.parent.GetComponent<EnemyStateMachine>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "PlayerWeapon" && attacking)
        {
            Debug.Log("Clash");

            enemy.SwitchState(enemy.enemyStun);
        }
        else if (collision.transform.tag == "Player" && attacking)
        {
            collision.transform.GetComponent<FirstPersonPlayer>().stun = true;
            collision.transform.GetComponent<FirstPersonPlayer>().knockback = new Vector3(transform.forward.x, 0, transform.forward.y);
        }
    }

}
