using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FleshMonsterAI : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float runSpeed = 14f;
    [SerializeField] float walkSpeed = 10f;
    [SerializeField] float perceptionRadius = 20;
    [SerializeField] float attackDistance = 4;

    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 startPosition;
    private float currentSpeed;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (!target) target = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;
        currentSpeed = 0;

    }

    void Update()
    {
        agent.speed = currentSpeed;
        animator.SetFloat("speed", currentSpeed);
        //agent.SetDestination(LookForPlayer());

    }


    Vector3 LookForPlayer()
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
        return startPosition;
    }

}
