using System;
using UnityEngine;

public class CharacterControls : MonoBehaviour
{
    //Components
    [Header("Components")]
    public Transform camera;
    public CharacterController controller;
    public Animator animator;
    public WallRunning wallRunning;
    

    //For Movement
    [Header("Movement")]
    [HideInInspector] public bool moveEnabled = true;
    Vector3 moveDirection;
    float moveSpeed = 20;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [HideInInspector] RaycastHit slopeHit;
    
    [Header("Looking")]
    Vector2 lookDirection;
    [HideInInspector] public bool lookEnabled = true;
    [Range(0,90)] public float maxLookY = 45;
    [Range(-90,0)] public float minLookY = -45;
    [SerializeField] float cameraBobSpeed = 5;
    [SerializeField] float cameraBobHeight = 0.25f;
    float rx = 0;
    float ry = 0;

    // For Jumping Around
    [Header("Jumping")]
    [SerializeField][Min(1)] float gravity = 10;
    [SerializeField][Min(0)] float groundDistance = 0.5f;
    [HideInInspector] public bool grounded;
    [Min(1)] public float jumpHeight = 3;
    bool jumping = false;
    


    //Events
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if(!wallRunning) wallRunning = GetComponent<WallRunning>();
    }

    void Update()
    {
        if(lookEnabled) LookAround();
        if(moveEnabled) Movement();
    }

    void FixedUpdate()
    {
        // Checks If player is grounded
        Ray groundRay = new Ray(transform.position, Vector3.down);
        grounded = Physics.Raycast(groundRay, out slopeHit, groundDistance);
    }

    //Functions
    void LookAround()
    {
        lookDirection = Game.controls.Player.Look.ReadValue<Vector2>();
        rx -= lookDirection.y * Game.aimSense * Time.deltaTime;
        rx = Mathf.Clamp(rx,minLookY,maxLookY);
        ry += lookDirection.x * Game.aimSense * Time.deltaTime;
        camera.transform.localEulerAngles = new Vector3(rx,0,0);
        transform.localEulerAngles = new Vector3(0,ry,0);
    }

    void Movement()
    {
        // When player hits the ground
        if (grounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        // Moving Around 
        float x = Game.controls.Player.Move.ReadValue<Vector2>().x;
        float z = Game.controls.Player.Move.ReadValue<Vector2>().y;
        float m = Game.controls.Player.Move.ReadValue<Vector2>().magnitude;
        moveDirection = (transform.right * x + transform.forward * z).normalized * m;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        
        //Gravity
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //Jumping
        if(Game.controls.Player.Jump.WasPressedThisFrame() && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2 * gravity);
        }

    }

    void ApplyDrag(float amount)
    {
        if(velocity.x > 1 || velocity.x < -1) velocity.x = Mathf.Lerp(velocity.x, 0, 5 * Time.deltaTime);
        else velocity.x = 0;
        
        if(velocity.z > 1 || velocity.z < -1) velocity.z = Mathf.Lerp(velocity.z, 0, 5 * Time.deltaTime);
        else velocity.z = 0;
    }

    

    //Animation Events
    

}
