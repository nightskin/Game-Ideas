using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    EnemyBaseState currentState;
    Vector3 velocity;
    Vector3 gravity = new Vector3(0, -9.81f, 0);

    public CharacterController controller;
    public Animator anim;
    public Transform armPivot;

    public EnemyPatrolState enemyPatrol = new EnemyPatrolState();
    public EnemyRetreatState enemyRetreat = new EnemyRetreatState();
    public EnemyChaseState enemyChase = new EnemyChaseState();
    public EnemyAtkState enemyAttack = new EnemyAtkState();
    public EnemyStunState enemyStun = new EnemyStunState();

    public Transform target;

    public float atkDistance = 2;

    void Start()
    {
        anim.GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        currentState = enemyPatrol;
        currentState.Start(this);
    }

    void Update()
    {
        currentState.Update(this);

        if(currentState != enemyStun)
        {
            if (!controller.isGrounded)
            {
                //Gravity
                velocity.y += gravity.y * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime);
            }
            else
            {
                velocity.y = 0;
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit collision)
    {
        currentState.CollisionEnter(this, collision);
    }

    public void SwitchState(EnemyBaseState state)
    {
        currentState = state;
        currentState.Start(this);
    }

    public void StartAttack()
    {
        float atkAngle = Random.Range(-135, 135);
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
    }

}
