using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public HealthScript health;
    public NavMeshAgent agent;


    
    void Start()
    {
        health = GetComponent<HealthScript>();
        agent = GetComponent<NavMeshAgent>();


    }

    
    void Update()
    {
        
    }
}
