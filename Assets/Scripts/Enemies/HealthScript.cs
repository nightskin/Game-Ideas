using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    [SerializeField] int maxHealth = 5;
    [SerializeField] Animator animator;
    [SerializeField] List<Collider> colliders = new List<Collider>();
    
    int health;

    private void Start()
    {
        if(!animator) animator = GetComponent<Animator>();
        colliders = GetComponentsInChildren<Collider>().ToList();

        for(int i = 0; i < colliders.Count; i++)
        {
            if (colliders[i].transform.tag == "LockAble")
            {
                colliders.RemoveAt(i);
            }
        }
        for (int i = 0; i < colliders.Count; i++)
        {
            if (colliders[i].transform.tag == "EnemyWeapon")
            {
                colliders.RemoveAt(i);
            }
        }



        SetRagDoll(false);
        health = maxHealth;
    }


    public void SetRagDoll(bool on)
    {
        if(on)
        {
            foreach(Collider collider in colliders) 
            {
                collider.transform.GetComponent<Rigidbody>().isKinematic = false;
                collider.isTrigger = false;
            }
            animator.enabled = false;
        }
        else
        {
            foreach (Collider collider in colliders)
            {
                collider.transform.GetComponent<Rigidbody>().isKinematic = true;
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
