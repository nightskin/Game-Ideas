using UnityEngine;
using UnityEngine.AI;

public class FleshMonsterPatrolState : FleshMonsterBaseState
{
    public override void Start(FleshMonsterAI ai)
    {
        Debug.Log("Patrol");
        ai.SetSpeed(ai.walkSpeed);
    }
        
    public override void Update(FleshMonsterAI ai)
    {
        if(ai.SeesPlayer())
        {
            ai.SwitchState(ai.chaseState);
        }

        if(ai.agent.remainingDistance <= ai.agent.stoppingDistance)
        {
            Vector3 randomPoint = ai.transform.position + Random.insideUnitSphere * ai.viewDistance;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, ai.viewDistance, NavMesh.AllAreas))
            {
                ai.agent.SetDestination(hit.position);
            }
        }
    }
}
