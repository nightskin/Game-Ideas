using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FleshMonsterAI : MonoBehaviour
{
    [SerializeField] EnemyLineOfSight lineOfSight;


    public Transform target;
    public float walkSpeed = 5f;
    public float perceptionRadius = 20;
    public float attackDistance = 4;
    public Animator animator;
    public NavMeshAgent agent;
    
    void Start()
    {
        if (!lineOfSight) transform.Find("LineOfSight");
        if(!agent)agent = GetComponent<NavMeshAgent>();
        if(!animator) animator = GetComponent<Animator>();
        if (!target) target = GameObject.FindGameObjectWithTag("Player").transform;

        agent.stoppingDistance = attackDistance;

    }

    void Update()
    {
        
    }


    void SetSpeed(float speed)
    {
        agent.speed = speed;
        animator.SetInteger("speed", (int)speed);
    }


}
