using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStunState : EnemyBaseState
{
    public Vector3 knockback;
    float stunTimer;
    public override void Start(EnemyStateMachine enemy)
    {
        stunTimer = 0.2f;
    }
    public override void Update(EnemyStateMachine enemy)
    {
        stunTimer -= Time.deltaTime;
        if(stunTimer <= 0)
        {
            enemy.SwitchState(enemy.enemyChase);
        }
        else
        {
            enemy.transform.position += knockback * 20 * Time.deltaTime;
        }
    }
    public override void CollisionEnter(EnemyStateMachine enemy)
    {
        
    }
}
