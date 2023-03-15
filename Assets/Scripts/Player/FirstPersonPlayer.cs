using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonPlayer : MonoBehaviour
{
    // For Input
    private Controls controls;
    public Controls.PlayerActions actions;
    
    //Components
    [SerializeField] private Transform camera;
    [SerializeField] private CharacterController controller;

    //For Ground Checking
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private bool isGrounded;
    private float groundDistance = 0.4f;

    // For basic motion
    private Vector3 motion;
    public float moveSpeed = 10;
    [SerializeField] private float wallDistance = 0.5f;

    //For Look/Aim
    public Vector2 lookSpeed = new Vector2(100f, 100);
    private float xRot = 0;
    private float yRot = 0;

    // For Jumping
    private Vector3 velocity;
    private Vector3 gravity = new Vector3(0,-9.81f, 0);
    public bool gravityOn = true;
    public float jumpHeight = 2;

    //For Dashing
    public float dashSpeed = 20;
    public float dashAmount = 1;

    Vector3 dashDirection;
    float dashTimer;
    bool dashing = false;

    void Awake()
    {
        dashTimer = dashAmount;
        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();
        actions.Jump.performed += Jump_performed;
        actions.Dash.performed += Dash_performed;
    }

    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(isGrounded)
        {
            if (motion == Vector3.zero)
            {
                dashDirection = transform.forward + new Vector3(0, 0.25f, 0);
            }
            else
            {
                dashDirection = motion + new Vector3(0, 0.25f, 0);
            }
            dashing = true;
        }

    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //Jumping
        if (isGrounded)
        {
            velocity.y =  Mathf.Sqrt(jumpHeight * -2 * gravity.y);
        }
    }

    void Update()
    {
        Look();
        Movement();

        if(dashing)
        {
            dashTimer -= Time.deltaTime;
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);

            if(dashTimer <= 0)
            {
                dashTimer = dashAmount;
                dashing = false;
            }
        }

    }

    void Movement()
    {
        if(gravityOn)
        {
            //Ground Checking
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }


        if(controller.isGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        motion = transform.right * x + transform.forward * z;
        if(!dashing)
        {
            controller.Move(motion * moveSpeed * Time.deltaTime);
        }

        if(gravityOn)
        {
            //Gravity
            velocity += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

    }

    void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;
        //Looking up/down with camera
        xRot -= y * lookSpeed.y * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90, 45);
        camera.localEulerAngles = new Vector3(xRot, 0, 0);
        //Looking left right with player body
        yRot += x * lookSpeed.x * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }
}
