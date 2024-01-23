using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIDeadState : EnemyAIState
{
    public override void Start(EnemyAI ai)
    {
        ai.agent.isStopped = true;
        ai.health.SetRagdoll(true);
        ai.lockOn.GetComponent<Rigidbody>().isKinematic = true;
        ai.lockOn.GetComponent<Collider>().enabled = false;
        ai.player.GetComponent<MeleeSystem>().lockOnTarget = null;
    }

    public override void Update(EnemyAI ai)
    {

    }
}
