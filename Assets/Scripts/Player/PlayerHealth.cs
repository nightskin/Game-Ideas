using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 100;
    [SerializeField] int health;
    BladeSystem meleeSystem;
    FirstPersonPlayer player;


    void Start()
    {
        if(!player) player = GetComponent<FirstPersonPlayer>();
        if(!meleeSystem) meleeSystem = GetComponent<BladeSystem>();
        health = maxHealth;
    }

    public void Heal(int amount)
    {
        health += amount;
        if (health >= maxHealth)
        {
            health = maxHealth;
            Debug.Log("Health Full");
        }
    }

    public void TakeDamage()
    {
        health--;
        if (health == 0)
        {
            Debug.Log("Player Dead");
        }
    }
    
}
