using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 5;
    [SerializeField] int health;

    [SerializeField] Transform rig;
    [SerializeField] Animator animator;



    void Start()
    {
        if(!animator) animator = GetComponent<Animator>();
        health = maxHealth;
        SetRagdoll(false);
    }

    public void TakeDamage()
    {
        health--;
        if(health <= 0) SetRagdoll(true);
    }

    public void SetRagdoll(bool active)
    {
        Rigidbody[] rigidbodies = rig.GetComponentsInChildren<Rigidbody>();
        Collider[] colliders = rig.GetComponentsInChildren<Collider>();

        if (active)
        {
            animator.enabled = false;
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = false;
            }
            foreach (Collider col in colliders)
            {
                col.isTrigger = false;
            }
        }
        else
        {
            animator.enabled = true;
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;
            }
            foreach (Collider col in colliders)
            {
                col.isTrigger = true;
            }
        }


    }
    
}
