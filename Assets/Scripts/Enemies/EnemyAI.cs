using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    EnemyAIState currentState;
    
    public EnemyChaseState chaseState = new EnemyChaseState();
    public EnemyPatrolState patrolState = new EnemyPatrolState();
    public EnemyAttackState fightState = new EnemyAttackState();
    public EnemyAIDeadState deadState = new EnemyAIDeadState();

    public NavMeshAgent agent;
    public Animator animator;
    public Transform head;
    public Transform player;
    public float attackDistance = 3;

    void Start()
    {
        if(!agent) agent = GetComponent<NavMeshAgent>();
        if(!animator) animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentState = patrolState;
        currentState.Start(this);
    }

    void Update()
    {
        currentState.Update(this);
    }

    public void SwitchState(EnemyAIState state)
    {
        currentState = state;
        currentState.Start(this);
    }


    //Animation Events
    public void FinishAttack()
    {
        animator.SetInteger("atkAngle", 0);
        SwitchState(chaseState);
    }

}
