using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObiliskStunState : ObiliskBaseState
{
    float stunTimer = 0.2f;
    public override void Start(ObliliskAI enemy)
    {
        for (int i = 0; i < enemy.weapons.Length; i++)
        {
            enemy.weapons[i].attacking = false;
        }
        enemy.animator.SetTrigger("recoil");
        enemy.armPivot.localEulerAngles = Vector3.zero;
    }

    public override void Update(ObliliskAI enemy)
    {
        stunTimer -= Time.deltaTime;
        if(stunTimer <= 0)
        {
            enemy.SwitchState(enemy.EnemyEngage);
            stunTimer = 0.2f;
        }

    }
}
