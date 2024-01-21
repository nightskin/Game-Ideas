using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : EnemyAIState
{
    float patrolRange = 20;
    float patrolSpeed = 3.5f;

    public override void Start(EnemyAI ai)
    {
        ai.agent.speed = patrolSpeed;
    }

    public override void Update(EnemyAI ai)
    {
        ai.animator.SetFloat("speed", ai.agent.velocity.magnitude);
        if (ai.agent.remainingDistance <= ai.agent.stoppingDistance) 
        {
            Vector3 randomPoint = ai.transform.position + Random.insideUnitSphere * patrolRange;
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(randomPoint, out navMeshHit, 1.0f, NavMesh.AllAreas))
            {
                ai.agent.SetDestination(navMeshHit.position);
            }
        }

        if(Physics.Raycast(ai.head.position,ai.head.forward, out RaycastHit rayHit))
        {
            if(rayHit.transform.tag == "Player")
            {
                ai.SwitchState(ai.chaseState);
            }
        }

    }


}
