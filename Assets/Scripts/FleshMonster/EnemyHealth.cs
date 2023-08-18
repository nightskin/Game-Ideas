using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 3;
    [SerializeField] int health;

    void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage()
    {
        health--;
        if(health <= 0)
        {
            GetComponent<FleshMonsterAI>().SetDeadState(true);
            GetComponent<NavMeshAgent>().enabled = false;
            GetComponent<FleshMonsterAI>().enabled = false;
        }
    }

    
}
