using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshMonsterAttackState : FleshMonsterBaseState
{
    float atkDelay = 0.5f;
    float atkTimer;
    public override void Start(FleshMonsterAI ai)
    {
        atkTimer = atkDelay;
        ai.SetSpeed(0);
        ai.Attack();
    }
    
    public override void Update(FleshMonsterAI ai)
    {
        Quaternion look = Quaternion.LookRotation(ai.player.position - ai.transform.position);
        ai.transform.rotation = Quaternion.Lerp(ai.transform.rotation, look, 10 * Time.deltaTime);

        atkTimer -= Time.deltaTime;
        if(atkTimer <= 0)
        {
            ai.Attack();
            atkTimer = atkDelay;
        }

        if (!ai.SeesPlayer())
        {
            ai.animator.SetInteger("atkAngle", 0);
            ai.SwitchState(ai.patrolState);
        }
        else if(Vector3.Distance(ai.transform.position, ai.player.position + (ai.player.forward)) > ai.attackDistance)
        {
            ai.animator.SetInteger("atkAngle", 0);
            ai.SwitchState(ai.chaseState);
        }
    }


}
