using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshMonsterAttackState : FleshMonsterBaseState
{
    float attackRate = 1;
    float attackTimer = 0;

    public override void Start(FleshMonsterAI ai)
    {
        ai.SetSpeed(0);
    }
    
    public override void Update(FleshMonsterAI ai)
    {
        Vector3 directionToPlayer = (ai.player.position - ai.transform.position).normalized;
        ai.transform.rotation = Quaternion.Lerp(ai.transform.rotation, Quaternion.LookRotation(directionToPlayer), 10 * Time.deltaTime);


        attackTimer -= Time.deltaTime;
        if(attackTimer <= 0)
        {
            Attack(ai);
            attackTimer = attackRate;
        }
        else
        {
            ai.animator.SetInteger("atkAngle", 0);
        }

        if (Vector3.Distance(ai.player.position, ai.transform.position) > ai.attackDistance || !ai.SeesPlayer())
        {
            ai.SwitchState(ai.chaseState);
        }
    }


    void Attack(FleshMonsterAI ai)
    {
        int attackAngle = Random.Range(1, 5);
        ai.animator.SetInteger("atkAngle", attackAngle);

    }

}
