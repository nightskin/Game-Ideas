using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAtkState : EnemyBaseState
{
    public override void Start(EnemyStateMachine enemy)
    {
        enemy.anim.SetTrigger("atk");
    }
    public override void Update(EnemyStateMachine enemy)
    {
        
    }
    public override void CollisionEnter(EnemyStateMachine enemy, ControllerColliderHit other)
    {

    }
}
