using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    [SerializeField] int maxHealth = 5;    
    
    int health;


    private void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage()
    {
        maxHealth--;
    }

    public bool IsDead()
    {
        return health <= 0;
    }

}
