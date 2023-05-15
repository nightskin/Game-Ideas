using UnityEngine;
using UnityEngine.AI;

public class ObliliskAI : MonoBehaviour
{
    public Animator animator;
    public EnemyWeapon[] weapons;
    public NavMeshAgent agent;
    public Transform target;
    public Transform armPivot;

    public float combatDistance = 10;


    ObiliskBaseState currentState;

    public ObiliskStunState EnemyStun = new ObiliskStunState();
    public ObiliskFollowState EnemyFollow = new ObiliskFollowState();
    public ObiliskCombatState EnemyEngage = new ObiliskCombatState();
    public ObiliskAttackState EnemyAttack = new ObiliskAttackState();

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        weapons = armPivot.GetComponentsInChildren<EnemyWeapon>();
        currentState = EnemyFollow;
        SwitchState(currentState);  
    }

    void Update()
    {
        currentState.Update(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        
    }

    public void SwitchState(ObiliskBaseState state)
    {
        currentState = state;
        state.Start(this);
    }

    public void StartAttack()
    {
        for(int i = 0; i < weapons.Length; i++)
        {
           weapons[i].attacking = true;
        }
    }

    public void EndAttack()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].attacking = false;
        }
    }

}
