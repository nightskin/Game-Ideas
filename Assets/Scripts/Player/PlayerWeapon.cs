using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon: MonoBehaviour
{
    public FirstPersonPlayer player;
    public MeleeSystem meleeSystem;
    public bool attacking;
    public int damage = 1;
    public float knockback = 100;
    


    private void OnCollisionEnter(Collision other)
    {
        if(other.transform.tag == "EnemyWeapon" && attacking)
        {
            player.stun = true;
        }
        if(other.transform.tag == "Enemy" && attacking)
        {
            var enemyStateMachine = other.transform.GetComponent<EnemyStateMachine>();
            enemyStateMachine.enemyStun.knockback = new Vector3(transform.forward.x, 0, transform.forward.y);
            enemyStateMachine.SwitchState(enemyStateMachine.enemyStun);
        }

    }

}
