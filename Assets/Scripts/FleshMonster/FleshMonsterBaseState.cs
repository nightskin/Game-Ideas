using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FleshMonsterBaseState
{
    public abstract void Start(FleshMonsterAI ai);
    public abstract void Update(FleshMonsterAI ai);

}
