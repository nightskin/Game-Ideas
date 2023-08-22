using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FleshMonsterHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 10;
    [SerializeField] int health;

    void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        health-= damage;
        if(health <= 0)
        {
            GetComponent<FleshMonsterAI>().SetDeadState(true);
            GetComponent<NavMeshAgent>().enabled = false;
            GetComponent<FleshMonsterAI>().enabled = false;
        }
    }

    
}
