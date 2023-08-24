using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshMonsterAttackState : FleshMonsterBaseState
{
    public override void Start(FleshMonsterAI ai)
    {
        ai.agent.isStopped = true;
        ai.Attack();
    }
    
    public override void Update(FleshMonsterAI ai)
    {
        if (Vector3.Distance(ai.transform.position, ai.player.position) > ai.agent.stoppingDistance)
        {
            ai.SwitchState(ai.chaseState);
        }


        Quaternion look = Quaternion.LookRotation(ai.player.position - ai.transform.position);
        ai.transform.rotation = Quaternion.Lerp(ai.transform.rotation, look, 10 * Time.deltaTime);


        if(ai.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            int r = Random.Range(0, 2);
            if (r == 0) ai.Attack();
            else if (r == 1) ai.animator.SetInteger("atkAngle", 0); ai.animator.SetInteger("speed", 0);
        }




    }


}
