using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Components
    [Header("Components")]
    Controls controls;
    public Controls.PlayerActions actions;
    public Camera camera;
    public CharacterController controller;
    public PlayerCombatControls combatControls;

    //For Basic Controls
    [Header("General")]
    [SerializeField][Min(1)] float moveSpeed = 10;
    Vector3 moveDirection;
    public float lookSpeed = 100;

    float xRot = 0;
    float yRot = 0;


    //For Jumping and falling
    [Header("Jumping")]
    [SerializeField] float jumpHeight = 3;
    [SerializeField] LayerMask jumpLayer;
    [SerializeField] float gravity = 10.0f;
    bool grounded = false;
    Vector3 velocity = Vector3.zero;

    //LockOn System
    [Header("LockOnSystem")]
    [SerializeField] float lockOnDistance = 500;
    [SerializeField] LayerMask lockOnLayer;
    Transform mainTarget = null;


    void Awake()
    {
        combatControls = GetComponent<PlayerCombatControls>();
        controller = GetComponent<CharacterController>();
        camera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();

        actions.Jump.performed += Jump_performed;
        actions.LockOn.performed += LockOn_performed;
    }

    void Update()
    {
        if(mainTarget != null)
        {
            LookAtTarget();
        }
        else
        {
            LookAround();
        }

        Movement();
    }

    void FixedUpdate()
    {
        grounded = Physics.CheckSphere(transform.position + (Vector3.down * controller.height / 2), controller.radius, jumpLayer);
    }

    void OnDestroy()
    {
        actions.Jump.performed -= Jump_performed;

    }
    
    private void Jump_performed(InputAction.CallbackContext obj)
    {
        if (grounded)
        {
            velocity = Vector3.up * Mathf.Sqrt(jumpHeight * 2 * gravity);
        }
    }

    private void LockOn_performed(InputAction.CallbackContext context)
    {
        
    }

    void Movement()
    {

        if(grounded && velocity.y < 0) 
        {
            velocity = Vector3.zero;
        }
        
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = (transform.right * x + transform.forward * z).normalized;
        combatControls.animator.SetFloat("speed", moveDirection.magnitude);
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);



        velocity += Vector3.down * gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void LookAround()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;

        //Looking up/down with camera
        xRot -= y * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -45, 45);
        camera.transform.localEulerAngles = new Vector3(xRot, 0, 0);
        
        //Looking left right with player body
        yRot += x * lookSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);

    }
    
    void LookAtTarget()
    {
        
    }
}
