using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Components
    [Header("Components")]
    public PlayerHUD hud;
    public Camera camera;
    public CharacterController controller;
    public PlayerCombatControls combatControls;
    
    //For Basic Controls
    [Header("General")]
    float moveSpeed;
    [SerializeField][Min(1)] float walkSpeed = 25;
    [SerializeField][Min(2)] float runSpeed = 50;
    [HideInInspector] public bool isCrouching = false;
    [SerializeField] float crouchSpeed = 5;
    [HideInInspector] public float lookSpeed;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [SerializeField][Min(0)] float cameraBobSpeed = 1f;
    RaycastHit slopeHit;
    float xRot = 0;
    float yRot = 0;
    Vector3 moveDirection;

    // For Jumping Around
    [Header("Jumping Variables")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField][Min(0)] float groundDistance = 0.5f;
    [HideInInspector] public bool grounded;
    [SerializeField] bool jumpingEnabled;
    [SerializeField][Min(0)] int maxJumps = 1;
    [SerializeField][Min(1)] float jumpHeight = 3;
    int numberOfJumps = 0;
    bool jumping = false;


    [Header("Dashing")]
    bool dashing = false;
    [SerializeField] float dashSpeed = 150;
    float dashTime = 0.1f;
    float dashTimer = 0;
    Vector3 dashDirection;

    //LockOn System
    [Header("LockOnSystem")]
    [SerializeField] bool lockOnSystemEnabled = true;
    [SerializeField] float lockOnDistance = 500;
    [SerializeField] LayerMask lockOnLayer;
    [HideInInspector] public Transform lockOnTarget = null;
    float lockOnLerp = 0;
    


    void Start()
    {
        moveSpeed = walkSpeed;
        lookSpeed = Game.mouseSensitivity;

        Cursor.lockState = CursorLockMode.Locked;

        Game.controls.Player.Jump.performed += Jump_performed;
        Game.controls.Player.Sprint.performed += Sprint_performed;
        Game.controls.Player.Sprint.canceled += Sprint_canceled;
        Game.controls.Player.Dash.performed += Dash_performed;
        Game.controls.Player.LockOn.performed += LockOn_performed;
        Game.controls.Player.Crouch.performed += Crouch_performed;
    }
    
    void Update()
    {
        if (lockOnTarget != null)
        {
            LookAtTarget();
        }
        else
        {
           MouseLook();
        }

        NormalMovement();


    }

    void FixedUpdate()
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        grounded = Physics.Raycast(groundRay, out slopeHit, groundDistance, groundLayer);
    }

    void OnDestroy()
    {
        Game.controls.Player.Jump.performed -= Jump_performed;
        Game.controls.Player.Sprint.performed -= Sprint_performed;
        Game.controls.Player.Sprint.canceled -= Sprint_canceled;
        Game.controls.Player.Dash.performed -= Dash_performed;
        Game.controls.Player.LockOn.performed -= LockOn_performed;
        Game.controls.Player.Crouch.performed -= Crouch_performed;
    }

    private void Jump_performed(InputAction.CallbackContext obj)
    {
        // Normal Jump
        if (jumpingEnabled)
        {
            if (numberOfJumps < maxJumps)
            {
                velocity = Vector3.up * Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
                numberOfJumps++;
                jumping = true;
                return;
            }
        }
    }

    private void Dash_performed(InputAction.CallbackContext obj)
    {
        if (!dashing)
        {
            dashDirection = moveDirection.normalized;
            dashing = true;
            dashTimer = dashTime;
        }
    }

    private void Sprint_performed(InputAction.CallbackContext obj)
    {
        moveSpeed = runSpeed;
    }

    private void Sprint_canceled(InputAction.CallbackContext obj)
    {
        moveSpeed = walkSpeed;
    }

    private void LockOn_performed(InputAction.CallbackContext obj)
    {
        if (lockOnSystemEnabled)
        {
            if (lockOnTarget == null)
            {
                if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, lockOnDistance, lockOnLayer))
                {
                    hud.animator.SetBool("lock", true);
                    lockOnTarget = hit.transform;
                }
            }
            else
            {
                lockOnTarget = null;
                hud.animator.SetBool("lock", false);
                lockOnLerp = 0;
            }
        }
    }

    private void Crouch_performed(InputAction.CallbackContext obj)
    {
        if (grounded)
        {
            if (isCrouching)
            {
                Crouch(true);
            }
            else
            {
                Crouch(false);
            }
        }
    }

    public void Crouch(bool activated)
    {
        if (activated)
        {
            camera.transform.localPosition = new Vector3(0, 1, 0);
            controller.center = new Vector3(0, 0.5f, 0);
            controller.height = 1;
            isCrouching = false;
            moveSpeed = walkSpeed;
        }
        else
        {
            camera.transform.localPosition = new Vector3(0, 2, 0);
            controller.center = new Vector3(0, 1, 0);
            controller.height = 2;
            isCrouching = true;
            moveSpeed = crouchSpeed;
        }
    }

    void NormalMovement()
    {
        if (grounded && velocity.y < 0)
        {
            numberOfJumps = 0;
            velocity = Vector3.zero;
            if(jumping) jumping = false;
        }

        float x = Game.controls.Player.Move.ReadValue<Vector2>().x;
        float z = Game.controls.Player.Move.ReadValue<Vector2>().y;
        float m = Game.controls.Player.Move.ReadValue<Vector2>().magnitude;
        moveDirection = (transform.right * x + transform.forward * z).normalized * m;

        if(m > 0)
        {
            combatControls.animator.SetBool("moving", true);
        }
        else
        {
            combatControls.animator.SetBool("moving", false);
        }

        //Move Input
        if (dashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer > 0)
            {
                controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            }
            else
            {
                dashing = false;
            }
        }
        else
        {
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        //Gravity
        velocity += new Vector3(0, -10, 0) * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //Handle Moving Down slopes
        if (grounded && !jumping)
        {
            controller.Move(new Vector3(0, -slopeHit.distance, 0));
        }
    }
    
    void MouseLook()
    {
        float x = Game.controls.Player.Look.ReadValue<Vector2>().x;
        float y = Game.controls.Player.Look.ReadValue<Vector2>().y;

        //Looking up/down with camera
        xRot -= y * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -45, 45);
        camera.transform.localRotation = Quaternion.Euler(xRot, 0, 0);

        //Looking left right with player body
        yRot += x * lookSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, yRot, 0);

    }

    void LookAtTarget()
    {
        Vector3 dirToTarget = lockOnTarget.position - camera.transform.position;
        Vector3 rotToTarget = Quaternion.LookRotation(dirToTarget).eulerAngles;


        if (lockOnLerp < 1)
        {
            lockOnLerp += Time.deltaTime;
            xRot = Mathf.LerpAngle(xRot, rotToTarget.x, lockOnLerp);
            yRot = Mathf.LerpAngle(yRot, rotToTarget.y, lockOnLerp);
        }
        else
        {
            xRot = rotToTarget.x;
            if (xRot > 359) xRot = 0;
            yRot = rotToTarget.y;
        }

        camera.transform.localRotation = Quaternion.Euler(xRot, 0, 0);
        transform.rotation = Quaternion.Euler(0, yRot, 0);
    }

}
