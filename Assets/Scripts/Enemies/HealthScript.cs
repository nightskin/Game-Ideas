using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    [SerializeField] int maxHealth = 5;
    [SerializeField] Animator animator;
    [SerializeField] Collider[] colliders;
    
    int health;




    private void Start()
    {
        animator = transform.root.GetComponent<Animator>();
        if(!animator) animator = GetComponent<Animator>();
        colliders = GetComponentsInChildren<Collider>();
        SetRagDoll(false);


        health = maxHealth;
    }


    public void SetRagDoll(bool on)
    {
        if(on)
        {
            foreach(Collider collider in colliders) 
            {
                collider.isTrigger = false;
            }
            animator.enabled = false;
        }
        else
        {
            foreach (Collider collider in colliders)
            {
                collider.isTrigger = true;
            }
            animator.enabled = true;
        }
    }

    public void TakeDamage()
    {
        health--;
        Debug.Log(health);
    }

    public bool IsDead()
    {
        return health <= 0;
    }

}
