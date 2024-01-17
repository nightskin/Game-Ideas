using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 10;
    [SerializeField] Rigidbody rigidbody;
    int health;


    void Start()
    {
        health = maxHealth;
        if(!rigidbody) rigidbody = GetComponent<Rigidbody>();
    }

    public void TakeDamage()
    {
        health--;
        if(health <= 0)
        {
            rigidbody.constraints = RigidbodyConstraints.None;
        }
    }

    
}
