using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshMonsterChaseState : FleshMonsterBaseState
{
    float chaseDuration = 5;
    float chaseTimer;
    public override void Start(FleshMonsterAI ai)
    {
        chaseTimer = chaseDuration;
        ai.SetSpeed(ai.runSpeed);
    }

    public override void Update(FleshMonsterAI ai)
    {
        ai.agent.SetDestination(ai.player.position + (ai.player.forward * ai.attackDistance));
        chaseTimer -= Time.deltaTime;
        if(chaseTimer <= 0)
        {
            if(!ai.SeesPlayer())
            {
                ai.SwitchState(ai.patrolState);
            }
            chaseTimer = chaseDuration;
        }


        if(Vector3.Distance(ai.transform.position, ai.player.position + (ai.player.forward)) <= ai.attackDistance)
        {
            ai.SwitchState(ai.attackState);
        }

    }
}
