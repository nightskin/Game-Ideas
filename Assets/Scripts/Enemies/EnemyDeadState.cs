using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIDeadState : EnemyAIState
{
    public override void Start(EnemyAI ai)
    {
        ai.agent.isStopped = true;
        ai.health.SetRagdoll(true);
        ai.walker.enabled = false;
    }

    public override void Update(EnemyAI ai)
    {

    }
}
