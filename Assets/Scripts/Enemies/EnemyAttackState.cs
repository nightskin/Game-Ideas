using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackState : EnemyAIState
{
    float lookSpeed = 10;

    public override void Start(EnemyAI ai)
    {
        ai.animator.SetFloat("speed", 0);
        ai.agent.isStopped = true;
        ai.animator.SetInteger("atkAngle", Random.Range(1, 3));

        Quaternion lookRot = Quaternion.LookRotation(ai.player.position - ai.transform.position);
        ai.transform.rotation = lookRot;

    }

    public override void Update(EnemyAI ai)
    {
        Quaternion lookRot = Quaternion.LookRotation(ai.player.position - ai.transform.position);
        ai.transform.rotation = Quaternion.Lerp(ai.transform.rotation, lookRot, lookSpeed * Time.deltaTime);
    }
}
