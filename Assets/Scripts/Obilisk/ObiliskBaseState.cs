using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObiliskBaseState
{
    public abstract void Start(ObliliskAI enemy);
    public abstract void Update(ObliliskAI enemy);

}
