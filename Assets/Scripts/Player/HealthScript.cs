using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    [SerializeField] float maxHealth = 100;
    float health;


    void Start()
    {
        health = maxHealth;
    }
    
    public bool IsDead()
    {
        return health <= 0;
    }

    public bool IsAlive()
    {
        return health > 0;
    }

    public void Heal(float amount)
    {
        health += amount;
        if (health >= maxHealth)
        {
            health = maxHealth;
            Debug.Log("Health Full");
        }
    }

    public void TakeDamage(float amount)
    {
        health-= amount;
        if (health <= 0)
        {
            Debug.Log("Player Dead");
        }
    }
    
}
