using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    float fov = 5;

    public override void Start(EnemyStateMachine enemy)
    {
        
    }
    public override void Update(EnemyStateMachine enemy)
    {
        if(Vector3.Distance(enemy.transform.position, enemy.target.position) <= fov)
        {
            enemy.SwitchState(enemy.enemyChase);
        }
    }
    public override void CollisionEnter(EnemyStateMachine enemy, ControllerColliderHit other)
    {

    }
}
