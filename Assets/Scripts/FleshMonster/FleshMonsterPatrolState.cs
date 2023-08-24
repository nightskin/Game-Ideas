using UnityEngine;
using UnityEngine.AI;

public class FleshMonsterPatrolState : FleshMonsterBaseState
{
    public override void Start(FleshMonsterAI ai)
    {
        if(ai.IsAlive()) ai.agent.isStopped = false;
        if(ai.IsAlive())ai.animator.SetInteger("atkAngle", 0);
        ai.agent.speed = ai.walkSpeed;
    }
        
    public override void Update(FleshMonsterAI ai)
    {
        //Patrolling Behaviour
        if (ai.agent.remainingDistance <= ai.agent.stoppingDistance)
        {
            Vector3 randomPoint = ai.transform.position + Random.insideUnitSphere * ai.viewDistance;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, ai.viewDistance, NavMesh.AllAreas))
            {
                ai.agent.SetDestination(hit.position);
            }
        }


        if (ai.SeesPlayer())
        {
            ai.SwitchState(ai.chaseState);
        }
    }
}
