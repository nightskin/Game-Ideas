using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FleshMonsterAI : MonoBehaviour
{
    private FleshMonsterBaseState currentState;
    public FleshMonsterAttackState attackState = new FleshMonsterAttackState();
    public FleshMonsterPatrolState patrolState = new FleshMonsterPatrolState();
    public FleshMonsterChaseState chaseState = new FleshMonsterChaseState();

    public Transform target;
    public float runSpeed = 14f;
    public float walkSpeed = 5f;
    public float perceptionRadius = 20;
    public float attackDistance = 4;
    public Animator animator;
    public NavMeshAgent agent;




    void Start()
    {
        if(!agent)agent = GetComponent<NavMeshAgent>();
        if(!animator) animator = GetComponent<Animator>();
        if (!target) target = GameObject.FindGameObjectWithTag("Player").transform;

        currentState = patrolState;
        currentState.Start(this);
    }

    void Update()
    {
        currentState.Update(this);
    }

    public void SwitchState(FleshMonsterBaseState state)
    {
        currentState = state;
        currentState.Start(this);
    }

    public void SetSpeed(float speed)
    {
        agent.speed = speed;
        animator.SetFloat("speed", speed);
    }

    public Vector3 LookForPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, perceptionRadius);
        if(colliders.Length > 0)
        {
            for(int c = 0; c < colliders.Length; c++)
            {
                if(colliders[c].transform == target)
                {
                    return colliders[c].transform.position;
                }
            }
        }
        return transform.position;
    }

}
