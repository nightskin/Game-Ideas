using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolState : EnemyBaseState
{
    public float fov = 20;
    Vector3[] patrolPoints = new Vector3[0];
    Vector3 destination;
    float patrolSpeed = 10;
    int patrolIndex = 0;

    public override void Start(EnemyStateMachine enemy)
    {
        if(patrolPoints.Length > 0)
        {
            destination = patrolPoints[patrolIndex];
            
        }
    }
    public override void Update(EnemyStateMachine enemy)
    {
        if(Physics.Raycast(enemy.transform.position, enemy.transform.forward, out RaycastHit hit))
        {
            if(hit.transform.tag == "Player")
            {
                enemy.SwitchState(enemy.enemyChase);
            }
        }
        if (patrolPoints.Length > 0)
        {
            if (Vector3.Distance(enemy.transform.position, destination) > 0)
            {
                Vector3 lookV = destination - enemy.transform.position;
                Quaternion rot = Quaternion.LookRotation(new Vector3(lookV.x, 0, lookV.z));
                enemy.transform.rotation = Quaternion.Lerp(enemy.transform.rotation, rot, patrolSpeed * Time.deltaTime);
                enemy.transform.position += enemy.transform.forward * patrolSpeed * Time.deltaTime;
            }
            else
            {
                if (patrolIndex < patrolPoints.Length - 1)
                {
                    patrolIndex++;
                }
                else
                {
                    patrolIndex = 0;
                }
                destination = patrolPoints[patrolIndex];
            }
        }
    }
    public override void CollisionEnter(EnemyStateMachine enemy, ControllerColliderHit other)
    {

    }
}
