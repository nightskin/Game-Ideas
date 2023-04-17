using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    float chaseSpeed = 10;
    float atkTimer = 0;
    public override void Start(EnemyStateMachine enemy)
    {

    }
    public override void Update(EnemyStateMachine enemy)
    {
        if (Vector3.Distance(enemy.transform.position, enemy.target.position) <= enemy.atkDistance)
        {
            atkTimer -= Time.deltaTime;
            if (atkTimer <= 0)
            {
                enemy.SwitchState(enemy.enemyAttack);
                atkTimer = 0.5f;
            }
        }
        else
        {
            Vector3 lookV = enemy.target.position - enemy.transform.position;
            Quaternion rot = Quaternion.LookRotation(new Vector3(lookV.x, 0, lookV.z));
            enemy.transform.rotation = Quaternion.Lerp(enemy.transform.rotation, rot, chaseSpeed * Time.deltaTime);
            enemy.transform.position += new Vector3(enemy.transform.forward.x, 0, enemy.transform.forward.z) * chaseSpeed * Time.deltaTime;
        }

    }
    public override void CollisionEnter(EnemyStateMachine enemy)
    {

    }
}
