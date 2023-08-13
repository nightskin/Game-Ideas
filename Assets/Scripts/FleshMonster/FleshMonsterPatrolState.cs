using UnityEngine;
using UnityEngine.AI;

public class FleshMonsterPatrolState : FleshMonsterBaseState
{

    public override void Start(FleshMonsterAI ai)
    {
        ai.SetSpeed(ai.walkSpeed);
        Debug.Log("Patrol");
    }

    public override void Update(FleshMonsterAI ai)
    {
        if(ai.LookForPlayer() != ai.transform.position)
        {
            Debug.Log("PlayerFound");
            ai.SwitchState(ai.chaseState);
        }
        
        if(ai.agent.remainingDistance <= ai.agent.stoppingDistance)
        {
            Vector3 randomPoint = ai.transform.position + Random.insideUnitSphere * ai.perceptionRadius;
            NavMeshHit hit;
            if(NavMesh.SamplePosition(randomPoint,out hit,1.0f,NavMesh.AllAreas))
            {
                ai.agent.SetDestination(hit.position);
            }
        }

    }


}
