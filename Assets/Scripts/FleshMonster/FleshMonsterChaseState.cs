using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshMonsterChaseState : FleshMonsterBaseState
{
    public override void Start(FleshMonsterAI ai)
    {
        ai.SetSpeed(ai.runSpeed);
        Debug.Log("Chasing");
    }
   
    public override void Update(FleshMonsterAI ai)
    {
        ai.agent.SetDestination(ai.target.position);

        if(Vector3.Distance(ai.transform.position, ai.target.position) <= ai.attackDistance)
        {
            
            ai.SwitchState(ai.attackState);
        }


    }
}
