using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAtkState : EnemyBaseState
{
    public override void Start(EnemyStateMachine enemy)
    { 
       float atkAngle = Random.Range(-90, 90);
       atkAngle = Mathf.Round(atkAngle / 45) * 45;
       enemy.armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
       enemy.anim.SetTrigger("atk");
       enemy.SwitchState(enemy.enemyChase);
    }
    public override void Update(EnemyStateMachine enemy)
    {

    }
    public override void CollisionEnter(EnemyStateMachine enemy)
    {

    }
}
