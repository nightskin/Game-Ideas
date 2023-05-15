using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObiliskCombatState : ObiliskBaseState
{
    float combatTimer;
    float blockAngle;
    
    public override void Start(ObliliskAI enemy)
    {

    }

    public override void Update(ObliliskAI enemy) 
    {
        if(Vector3.Distance(enemy.transform.position, enemy.target.position) > enemy.combatDistance)
        {
            enemy.SwitchState(enemy.EnemyFollow);
        }
        else
        {
            combatTimer -= Time.deltaTime;
            if(combatTimer <= 0)
            {
                int r = Random.Range(0, 3);
                if (r == 0) // Attack
                {
                    enemy.SwitchState(enemy.EnemyAttack);
                }
                else if(r== 1) // Block
                {
                    blockAngle = Random.Range(0f, 360f);
                    blockAngle = Mathf.Round(blockAngle / 45) * 45;
                    enemy.armPivot.localEulerAngles = new Vector3(0, 0, blockAngle);
                }
                combatTimer = 1;
            }

        }

    }
}
