using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshMonsterChaseState : FleshMonsterBaseState
{


    public override void Start(FleshMonsterAI ai)
    {
        ai.SetSpeed(ai.walkSpeed);
        Debug.Log("Chasing");
    }

    public override void Update(FleshMonsterAI ai)
    {
        if(ai.SeesPlayer())
        {
            if(Vector3.Distance(ai.player.position, ai.transform.position) <= ai.attackDistance)
            {
                ai.SwitchState(ai.attackState);
            }
            else
            {
                ai.agent.SetDestination(ai.player.position + (ai.player.forward));
            }
        }
        else
        {
            ai.SwitchState(ai.patrolState);
        }
    }
}
