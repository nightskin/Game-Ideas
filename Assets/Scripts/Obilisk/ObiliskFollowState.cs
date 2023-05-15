using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObiliskFollowState : ObiliskBaseState
{
    
    public override void Start(ObliliskAI enemy)
    {
        enemy.agent.isStopped = false;
    }

    public override void Update(ObliliskAI enemy)
    {
        if(Vector3.Distance(enemy.transform.position, enemy.target.position) <= enemy.combatDistance)
        {
            enemy.agent.isStopped = true;
            enemy.SwitchState(enemy.EnemyEngage);
        }
        else
        {
            enemy.agent.destination = enemy.target.position;
        }

    }


}
