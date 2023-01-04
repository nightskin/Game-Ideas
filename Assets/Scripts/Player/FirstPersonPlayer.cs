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

    // 
    private Vector3 motion;
    public float moveSpeed = 10;

    //For Look/Aim
    public Vector2 lookSpeed = new Vector2(100f, 100);
    private float xRot = 0;
    private float yRot = 0;

    // For Jumping
    private Vector3 velocity;
    private float gravity = -9.81f;
    public float jumpHeight = 2;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();
    }

    void Update()
    {
        Look();
        Movement();
    }

    void Movement()
    {
        //Ground Checking
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(controller.isGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        motion = transform.right * x + transform.forward * z;
        controller.Move(motion * moveSpeed * Time.deltaTime);

        //Jumping
        if(actions.Jump.IsPressed() && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }

        //Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;
        //Looking up/down with camera
        xRot -= y * lookSpeed.y * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -45, 45);
        camera.localEulerAngles = new Vector3(xRot, 0, 0);
        //Looking left right with player body
        yRot += x * lookSpeed.x * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }
}
