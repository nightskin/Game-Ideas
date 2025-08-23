using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] float speed = 10;
    [SerializeField] float visibilityDistance;
    [SerializeField] float combatDistance = 50;
    [SerializeField] float minStrikingDistance = 2;
    [SerializeField] float maxStrikingDistance = 4;

    [SerializeField] LayerMask targetLayer;
    [SerializeField] PlayerCombatControls target;
    [SerializeField] Animator animator;


    bool canSeePlayer = false;
    float transitionTimer = 0;
    float transitionTime = 0;
    int strafeDirection;

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 1);
        Gizmos.DrawLine(transform.position, transform.position + (transform.forward * visibilityDistance));
    }

    void Start()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!target) target = GameObject.Find("Player").GetComponent<PlayerCombatControls>();
    }

    void FixedUpdate()
    {
        canSeePlayer = CheckLineOfSightToTarget();
    }

    void Update()
    {
        if (canSeePlayer || Vector3.Distance(target.transform.position, transform.position) <= combatDistance)
        {
            Fight();
        }
        else
        {
            Patrol();
        }
    }

    //States
    void Fight()
    {
        //Look At Target
        Vector3 lookDir = target.transform.position - transform.position;
        Quaternion lookRot = Quaternion.LookRotation(lookDir);
        transform.localEulerAngles = new Vector3(0, lookRot.eulerAngles.y, 0);

        float distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
        if (distanceToTarget > maxStrikingDistance)
        {
            Advance();
        }
        else if (distanceToTarget < minStrikingDistance)
        {
            Retreat();
        }
        else
        {
            if (transitionTimer < transitionTime)
            {
                transitionTimer += Time.deltaTime;
                Strafe();
            }
            else
            {
                int d = Random.Range(0, 3);
                if (d == 0)
                {
                    ChangeGaurd();
                }
                else
                {
                    Attack();
                }
                strafeDirection = Random.Range(-1, 2);
                transitionTime = Random.Range(0, 5);
                transitionTimer = 0;
            }
        }
    }

    void Patrol()
    {
        transform.eulerAngles += Vector3.up * 45 * Time.deltaTime;
    }

    //Patrol Functions
    bool CheckLineOfSightToTarget()
    {
        if (Physics.SphereCast(transform.position, 0.5f ,transform.forward, out RaycastHit hitInfo, visibilityDistance, targetLayer))
        {
            return true;
        }
        return false;
    }

    //Combat Functions
    void Strafe()
    {
        transform.position += transform.right * speed * Time.deltaTime * strafeDirection;
    }

    void Advance()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void Retreat()
    {
        transform.position -= transform.forward * speed * Time.deltaTime;
    }

    void Attack()
    {
        animator.SetTrigger("atk");
        animator.SetFloat("atkAngle", Random.Range(0f, 1f));
    }

    void ChangeGaurd()
    {
        animator.SetFloat("defAngle", Random.Range(0, 3));
    }
}
