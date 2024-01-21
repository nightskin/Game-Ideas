using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAIState
{
    public abstract void Start(EnemyAI ai);
    public abstract void Update(EnemyAI ai);
}
