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
        if(attacking)
        {
            if (other.transform.tag == "Enemy")
            {
                Instantiate(impactEffectEnemy, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                var e = other.GetComponent<EnemyStateMachine>();
                if (meleeSystem.atkAngle > 135 || meleeSystem.atkAngle < -135)
                {
                    e.enemyStun.knockback = Vector3.up * knockbackForce;
                }
                else if (meleeSystem.atkAngle < 45 || meleeSystem.atkAngle > -45)
                {
                    e.enemyStun.knockback = player.transform.forward + Vector3.down * knockbackForce;
                }
                e.SwitchState(e.enemyStun);
            }
            if (other.transform.tag == "EnemyWeapon")
            {
                Instantiate(impactEffectSolid, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                meleeSystem.animator.SetTrigger("recoil");
            }
        }
    }

}
