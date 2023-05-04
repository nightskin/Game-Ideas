using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonPlayer : MonoBehaviour
{
    // For Input
    Controls controls;
    public Controls.PlayerActions actions;
    
    //Components
    [SerializeField] Transform camera;
    [SerializeField] CharacterController controller;

    // For basic motion
    Vector3 motion;
    public float moveSpeed = 10;
    [SerializeField] private float wallDistance = 0.5f;

    //For Look/Aim
    public Vector2 lookSpeed = new Vector2(100f, 100);
    float xRot = 0;
    float yRot = 0;

    // For Jumping
    [SerializeField] Vector3 velocity;
    [SerializeField] LayerMask groundMask;
    Transform groundCheck;
    Vector3 gravity = new Vector3(0,-9.81f, 0);
    bool gravityOn = true;
    private bool isGrounded;
    public float jumpHeight = 2;
    private int jumps = 0;
    public int maxJumps = 1;


    //For Dashing
    public float dashSpeed = 20;
    public float dashAmount = 1;
    Vector3 dashDirection;
    float dashTimer;
    bool dashing = false;

    //For HealthSystem
    public bool stun = false;
    public Vector3 knockback;
    [SerializeField] float stunTime = 0.25f;
    float stunTimer = 0.2f;
    int health = 100;


    void Awake()
    {
        dashTimer = dashAmount;
        groundCheck = transform.Find("GroundCheck");
        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();
        actions.Jump.performed += Jump_performed;
        actions.Dash.performed += Dash_performed;
    }

    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(!dashing)
        {
            if (motion == Vector3.zero)
            {
                dashDirection = transform.forward;
            }
            else
            {
                dashDirection = motion;
            }
            dashing = true;
        }

    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //Jumping
        if (jumps < maxJumps)
        {
            velocity.y =  Mathf.Sqrt(jumpHeight * -2 * gravity.y);
            jumps++;
        }
    }

    void Update()
    {
        if (!stun)
        {
            Look();
            Movement();

            if (dashing)
            {
                dashTimer -= Time.deltaTime;
                controller.Move(dashDirection * dashSpeed * Time.deltaTime);

                if (dashTimer <= 0)
                {
                    dashTimer = dashAmount;
                    dashing = false;
                }
            }
        }
        else
        {
            stunTimer -= Time.deltaTime;
            controller.Move(knockback * dashSpeed * Time.deltaTime);
            if (stunTimer <= 0)
            {
                stun = false;
                stunTimer = stunTime;
            }
        }
    }

    void Movement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f,groundMask);

        if(isGrounded && velocity.y < 0)
        {
            jumps = 0;
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
