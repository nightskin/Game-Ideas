using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshMonsterStunState : FleshMonsterBaseState
{
    
    public override void Start(FleshMonsterAI ai)
    {
        ai.attacking = false;
        ai.animator.SetTrigger("recoil");
    }

    public override void Update(FleshMonsterAI ai)
    {
        if(ai.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        {
            ai.SwitchState(ai.attackState);
        }
    }
}
