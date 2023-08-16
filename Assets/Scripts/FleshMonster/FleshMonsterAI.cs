using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FleshMonsterAI : MonoBehaviour
{
    private FleshMonsterBaseState currentState;
    public FleshMonsterChaseState chaseState = new FleshMonsterChaseState();
    public FleshMonsterPatrolState patrolState = new FleshMonsterPatrolState();
    public FleshMonsterAttackState attackState = new FleshMonsterAttackState();


    [SerializeField] LayerMask playerMask;
    public Transform player;
    public float runSpeed = 10;
    public float walkSpeed = 5f;
    public float fieldOfView = 45;
    public float viewDistance = 20;
    public float attackDistance = 4;
    public Animator animator;
    public NavMeshAgent agent;
    
    void Start()
    {
        if(!agent)agent = GetComponent<NavMeshAgent>();
        if(!animator) animator = GetComponent<Animator>();
        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;

        agent.stoppingDistance = attackDistance;


        SwitchState(patrolState);
    }

    void Update()
    {
        currentState.Update(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "PlayerWeapon")
        {

        }
    }

    public void SetSpeed(float speed)
    {
        agent.speed = speed;
        animator.SetInteger("speed", (int)speed);
    }

    public void SwitchState(FleshMonsterBaseState state)
    {
        currentState = state;
        currentState.Start(this);
    }

    public bool SeesPlayer()
    {
        if(Vector3.Distance(player.position, transform.position) <= viewDistance)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            if(Vector3.Angle(transform.forward, directionToPlayer) < fieldOfView/2)
            {
                if(Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit ,viewDistance, playerMask))
                {
                    if(hit.transform == player)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool SeesPlayer2()
    {
        if (Vector3.Distance(player.position, transform.position) <= viewDistance)
        {
            return true;
        }
        return false;
    }
}
