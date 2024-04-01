using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float turnSpeed = 5;

    [SerializeField] float shootDistance = 10;
    Animator animator;


    void Start()
    {
        animator = GetComponent<Animator>();
        if(!target)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }


    }


    void Update()
    {
        if (Vector3.Distance(transform.position, target.position) > shootDistance)
        {
            FollowPlayer();
        }
        else
        {

        }

    }


    void FollowPlayer()
    {
        Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }



}
