using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshMonsterChaseState : FleshMonsterBaseState
{
    
    public override void Start(FleshMonsterAI ai)
    {
        if(ai.IsAlive()) ai.agent.isStopped = false;
        if(ai.IsAlive())ai.animator.SetInteger("atkAngle", 0);
        ai.SetSpeed(ai.runSpeed);
    }

    public override void Update(FleshMonsterAI ai)
    {
        Vector3 offset = ai.transform.forward * ai.agent.stoppingDistance;
        ai.agent.SetDestination(ai.player.position - offset);


        if(ai.agent.remainingDistance <= ai.agent.stoppingDistance)
        {
            ai.SwitchState(ai.attackState);
        }

    }
}
