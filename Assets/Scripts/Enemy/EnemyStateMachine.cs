using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    EnemyBaseState currentState;


    public EnemyPatrolState enemyPatrol = new EnemyPatrolState();
    public EnemyRetreatState enemyRetreat = new EnemyRetreatState();
    public EnemyChaseState enemyChase = new EnemyChaseState();
    public EnemyAtkState enemyAttack = new EnemyAtkState();
    public EnemyStunState enemyStun = new EnemyStunState();


    public Transform armPivot;
    public Animator anim;
    public Rigidbody rb;
    public Transform target;

    public float atkDistance = 4.5f;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        armPivot = transform.Find("HandPivot");
        anim = GetComponent<Animator>();        
        rb = GetComponent<Rigidbody>();
        currentState = enemyPatrol;
        currentState.Start(this);
    }

    void Update()
    {
        currentState.Update(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //currentState.CollisionEnter(this);

    }

    public void SwitchState(EnemyBaseState state)
    {
        currentState = state;
        currentState.Start(this);
    }


    public void StartAttack()
    {
       armPivot.transform.GetChild(0).GetChild(0).GetComponent<EnemyWeapon>().attacking = true;
    }

    public void EndAttack()
    {
        armPivot.transform.GetChild(0).GetChild(0).GetComponent<EnemyWeapon>().attacking = false;
    }

}
