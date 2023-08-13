using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshMonsterAttackState : FleshMonsterBaseState
{
    float atkDelay = 1.0f;
    float atkTimer;
    public override void Start(FleshMonsterAI ai)
    {
        atkTimer = 0;
        ai.SetSpeed(0);
        Debug.Log("Attacking");
    }

    public override void Update(FleshMonsterAI ai)
    {
        if(Vector3.Distance(ai.target.position, ai.transform.position) > ai.attackDistance)
        {
            ai.SwitchState(ai.chaseState);
        }

        atkTimer -= Time.deltaTime;
        if(atkTimer <= 0)
        {
            int r = Random.Range(1, 6);
            ai.animator.SetInteger("atkAngle", r);
            atkTimer = atkDelay;
        }

    }
}
