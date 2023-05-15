using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObiliskAttackState : ObiliskBaseState
{
    

    public override void Start(ObliliskAI enemy)
    {
        int atk = Random.Range(1, 3);
        enemy.animator.SetTrigger("atk");
        enemy.SwitchState(enemy.EnemyEngage);
    }

    public override void Update(ObliliskAI enemy)
    {
        

    }
}
