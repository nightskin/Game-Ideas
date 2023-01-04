using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform target;
    public NavMeshAgent agent;

    public float moveSpeed = 10;
    public float sightRange;
    public float atkRange = 3;
    public bool foundPlayer;
    public bool canAttackPlayer;


    private Animator animator;

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }


}
