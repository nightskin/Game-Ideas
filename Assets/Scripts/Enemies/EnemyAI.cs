using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] float speed = 20;
    [SerializeField] float visibilityDistance;
    [SerializeField] float minStrikingDistance = 2;
    [SerializeField] float maxStrikingDistance = 4;

     
    [SerializeField] PlayerCombatControls target;
    [SerializeField] EnemyWeapon weapon;
    [SerializeField] Animator animator;
    [SerializeField] Transform armPivot;
    [SerializeField] LayerMask targetMask;

    bool canSeePlayer = false;
    bool finishedCurrentAction = true;
    int currentAction = 0;
    float transition = 0;
    float atkAngle = 0;

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.position, visibilityDistance);
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
        Fight();
    }
    //Patrol Functions
    void Patrol()
    {
        transform.eulerAngles += Vector3.up * 45 * Time.deltaTime;
    }

    bool CheckLineOfSightToTarget()
    {
        if (Vector3.Distance(transform.position, target.transform.position) <= visibilityDistance)
        {
            Vector3 toTarget = target.transform.position - transform.position;
            if (Physics.Raycast(transform.position, toTarget, out RaycastHit hitInfo, visibilityDistance, targetMask))
            {
                if (hitInfo.transform.tag == "Player")
                {
                    return true;
                }
            }
        }
        return false;
    }

    //Combat Functions
    void Fight()
    {
        //Look Towards Player
        Vector3 lookDir = target.transform.position - transform.position;
        Quaternion lookRot = Quaternion.LookRotation(lookDir);
        transform.localEulerAngles = new Vector3(0, Mathf.LerpAngle(transform.localEulerAngles.y, lookRot.eulerAngles.y, 10 * Time.deltaTime), 0);

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
            if (finishedCurrentAction)
            {
                currentAction = Random.Range(0, 2);
                finishedCurrentAction = false;
                transition = 0;
            }
            else
            {
                if (currentAction == 0) Attack();
                else if (currentAction == 1) ChangeGaurd();
            }
        }
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
        if (transition == 0)
        {
            int a = Random.Range(0, 5);
            if (a == 0) atkAngle = 0;
            else if (a == 1) atkAngle = -90;
            else if (a == 2) atkAngle = 90;
            else if (a == 3) atkAngle = -45;
            else atkAngle = 45;
        }

        transition += Time.deltaTime;
        armPivot.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(armPivot.localEulerAngles.z, atkAngle, transition));
        if (armPivot.localEulerAngles.z == atkAngle)
        {
            animator.SetTrigger("atk");
        }

    }

    void ChangeGaurd()
    {
        if(transition == 0)
        {
            int g = Random.Range(0, 3);
            if (g == 0) animator.SetTrigger("defL");
            else if (g == 1) animator.SetTrigger("defR");
            else if (g == 2) animator.SetTrigger("defU");
        }
        transition += Time.deltaTime;
        if (transition >= 1)
        {
            EndCurrentAction();
        }

    }

    [SerializeField]
    void EndCurrentAction()
    {
        if (currentAction == 0) armPivot.localEulerAngles = Vector3.zero;
        finishedCurrentAction = true;
    }
}
