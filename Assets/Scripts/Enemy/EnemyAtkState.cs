using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAtkState : EnemyBaseState
{
    public override void Start(EnemyStateMachine enemy)
    {
        enemy.anim.SetBool("atk",true);
        
    }
    public override void Update(EnemyStateMachine enemy)
    {
        if(Vector3.Distance(enemy.transform.position, enemy.target.position) > enemy.atkDistance)
        {
            enemy.anim.SetBool("atk", false);
            enemy.SwitchState(enemy.enemyChase);
        }
    }
    public override void CollisionEnter(EnemyStateMachine enemy, ControllerColliderHit other)
    {

    }
}
