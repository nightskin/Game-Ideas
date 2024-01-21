using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyAIState
{
    float chaseSpeed = 15;


    public override void Start(EnemyAI ai)
    {
        ai.agent.speed = chaseSpeed;
        ai.agent.isStopped = false;
    }

    public override void Update(EnemyAI ai)
    {
        ai.animator.SetFloat("speed", ai.agent.velocity.magnitude);
        Vector3 offset = (-ai.transform.forward) * ai.attackDistance;
        ai.agent.SetDestination(ai.player.position + offset);
        if (ai.agent.remainingDistance <= ai.agent.stoppingDistance)
        {
            ai.SwitchState(ai.fightState);
        }

    }
}
