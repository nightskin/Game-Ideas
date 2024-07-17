using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public EnemyHealth health;
    public NavMeshAgent agent;


    
    void Start()
    {
        health = GetComponent<EnemyHealth>();
        agent = GetComponent<NavMeshAgent>();


    }

    
    void Update()
    {
        
    }
}
