using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 5;
    [SerializeField] int health;

    [SerializeField] Transform rig;
    [SerializeField] Animator animator;
    [SerializeField] EnemyAI enemyAI;

    private void Start()
    {
        if(!animator) animator = GetComponent<Animator>();
        if(!enemyAI)  enemyAI = GetComponent<EnemyAI>();
        health = maxHealth;
        SetRagdoll(false);
    }
    
    public void TakeDamage(int amount = 1)
    {
        health -= amount;
        if (health <= 0) 
        {
            enemyAI.SwitchState(enemyAI.deadState);
        }
    }

    public void SetRagdoll(bool on)
    {
        Rigidbody[] rigidbodies = rig.GetComponentsInChildren<Rigidbody>();
        Collider[] colliders = rig.GetComponentsInChildren<Collider>();

        if (on)
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
    
    public bool GetRagDollState()
    {
        return !animator.enabled;
    }

}
