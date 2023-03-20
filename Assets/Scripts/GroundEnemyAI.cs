using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundEnemyAI : MonoBehaviour
{
    public Transform[] obstacles;
    public Transform target;
    public float moveSpeed = 25;
    public float lineOfSight = 20;

    private Animator animator;
    private CharacterController controller;

    //For Ground Checking
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    private bool isGrounded;
    private float groundDistance = 0.4f;

    // For Jumping
    private Vector3 velocity;
    private Vector3 gravity = new Vector3(0, -9.81f, 0);
    public float jumpHeight = 2;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    
    void Update()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        if (Found())
        {
            animator.SetBool("moving", true);
            Quaternion direction = FollowTarget();
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, moveSpeed * Time.deltaTime);
            GetComponent<CharacterController>().Move(new Vector3(transform.forward.x, 0, transform.forward.z) * moveSpeed * Time.deltaTime);
        }
        else
        {
            animator.SetBool("moving", false);
        }

        //Gravity
        velocity += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }

    Quaternion FollowTarget()
    {
        return Quaternion.LookRotation(new Vector3(target.position.x, 0, target.position.z) - new Vector3(transform.position.x, 0, transform.position.z));
    }

    Quaternion AvoidObstacles()
    {
        return Quaternion.LookRotation(transform.position - target.position);
    }

    bool Found()
    {
        if (Vector3.Distance(transform.position, target.position) <= lineOfSight)
        {
            return true;
        }

        return false;
    }
    

}
