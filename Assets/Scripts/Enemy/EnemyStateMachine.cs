using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    EnemyBaseState currentState;

    public CharacterController controller;
    public Animator anim;

    public EnemyIdleState enemyIdle = new EnemyIdleState();
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
        currentState = enemyIdle;
        currentState.Start(this);
    }

    void Update()
    {
        currentState.Update(this);
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

}
