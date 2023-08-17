using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleshMonsterDeadState : FleshMonsterBaseState
{
    public override void Start(FleshMonsterAI ai)
    {
        ai.SetDeadState(true);
    }

    public override void Update(FleshMonsterAI ai)
    {
        
    }
}
