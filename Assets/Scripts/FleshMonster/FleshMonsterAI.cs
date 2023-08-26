using UnityEngine;
using UnityEngine.AI;

public class FleshMonsterAI : MonoBehaviour
{
    private FleshMonsterBaseState currentState;
    public FleshMonsterChaseState chaseState = new FleshMonsterChaseState();
    public FleshMonsterPatrolState patrolState = new FleshMonsterPatrolState();
    public FleshMonsterAttackState attackState = new FleshMonsterAttackState();
    public FleshMonsterDeadState deadState = new FleshMonsterDeadState();
    public FleshMonsterStunState stunState = new FleshMonsterStunState();


    [SerializeField] LayerMask playerMask;
    public Transform player;
    public bool attacking = false;
    public float runSpeed = 10;
    public float walkSpeed = 5f;
    public float fieldOfView = 45;
    public float viewDistance = 20;
    [SerializeField] float attackDistance = 1.5f;
    public Animator animator;
    public NavMeshAgent agent;

    void Start()
    {
        if(!agent)agent = GetComponent<NavMeshAgent>();
        if(!animator) animator = GetComponent<Animator>();
        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;

        agent.stoppingDistance = attackDistance;

        SetDeadState(false);
        SwitchState(patrolState);
    }

    void Update()
    {
        animator.SetFloat("speed", agent.velocity.magnitude);
        currentState.Update(this);
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
            if(Vector3.Angle(transform.forward, directionToPlayer) < fieldOfView)
            {
                if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, viewDistance, playerMask))
                {
                    if (hit.transform == player)
                    {
                        return true;
                    }
                }
            }

        }
        return false;
    }

    public void Attack()
    {
        int attackAngle = Random.Range(1, 5);
        animator.SetInteger("atkAngle", attackAngle);
    }

    public bool IsAlive()
    {
        return animator.enabled;
    }

    public void StartAttack()
    {
        attacking = true;
    }

    public void EndAttack()
    {
        attacking = false;
    }

    public void SetDeadState(bool dead)
    {
        if(dead)
        {
            Collider[] ragDollColliders = transform.GetComponentsInChildren<Collider>();
            for (int c = 0; c < ragDollColliders.Length; c++)
            {
                if(ragDollColliders[c] != GetComponent<Collider>())
                {
                    ragDollColliders[c].isTrigger = false;
                    ragDollColliders[c].attachedRigidbody.isKinematic = false;
                }
            }
            animator.enabled = false;
            GetComponent<Collider>().enabled = false;
        }
        else
        {
            Collider[] ragDollColliders = transform.GetComponentsInChildren<Collider>();
            for (int c = 0; c < ragDollColliders.Length; c++)
            {
                if (ragDollColliders[c] != GetComponent<Collider>())
                {
                    ragDollColliders[c].isTrigger = true;
                    ragDollColliders[c].attachedRigidbody.isKinematic = true;
                }
            }
            animator.enabled = true;
            GetComponent<Collider>().enabled = true;
        }
    }
}
