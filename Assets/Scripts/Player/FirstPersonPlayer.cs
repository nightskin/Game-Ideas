using UnityEngine;

public class FirstPersonPlayer : MonoBehaviour
{
    Controls controls;
    public Controls.PlayerActions actions;

    //Components
    public Transform camera;
    public CharacterController controller;

    //For Basic Motion/mouselook
    Vector3 moveDirection;
    float currentSpeed;
    float walkSpeed = 15;
    float runSpeed = 20;

    public float lookSpeed = 100;
    float xRot = 0;
    float yRot = 0;

    // For Jumping and falling
    int numberOfJumps = 0;
    [SerializeField] int maxJumps = 1;
    [SerializeField] float jumpHeight = 2;
    Vector3 velocity = Vector3.zero;
    Transform groundCheck;
    float gravity = 10.0f;
    bool isGrounded = false;
    
    //For Wall Jumping
    [SerializeField] float wallJumpPower = 10;
    [SerializeField] LayerMask wallMask;
    [SerializeField] float wallJumpTime = 0.5f;

    bool canWallJump;
    bool isWallJumping;
    Vector3 wallJumpDirection;

    float wallJumpTimer = 0;

    
    //For lockOn System
    Transform lockOnTarget = null;

    //For Dashing
    [SerializeField] bool canDash = true;
    [SerializeField] int maxAirDashes = 3;
    int airDashes = 0;
    Vector3 dashDirection;
    float dashTime = 0.1f;
    float dashTimer;
    float dashSpeed = 100;
    bool dashing = false;


    void Awake()
    {
        dashTimer = dashTime;
        currentSpeed = walkSpeed;
        if (!camera) camera = transform.Find("Camera");
        if(!groundCheck) groundCheck = transform.Find("GroundCheck");
        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();

        actions.Dash.performed += Dash_performed;
        actions.Jump.performed += Jump_performed;
        actions.Jump.canceled += Jump_canceled;
        actions.Run.performed += Run_performed;
        actions.Run.canceled += Run_canceled;
        actions.LockOn.performed += LockOn_performed;
        actions.LockOn.canceled += LockOn_canceled;
    }

    void Update()
    {
        if(dashing)
        {
            Dash();
        }
        else
        {
            if (!lockOnTarget)
            {
                Look();
                if(isWallJumping)
                {
                    WallJump();
                }
                else
                {
                    Movement();
                }
            }
            else if (lockOnTarget)
            {
                LookAtTarget();
                LockOnMovement();
            }
        }



    }

    void FixedUpdate()
    {
        //Check If Player Can Jump
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, 0.25f);

        if(!isGrounded)
        {
            canWallJump = Physics.CheckSphere(transform.position, controller.radius + 0.5f, wallMask);
        }

    }

    void OnDestroy()
    {
        actions.Dash.performed -= Dash_performed;
        actions.Jump.performed -= Jump_performed;
        actions.Jump.canceled -= Jump_canceled;
        actions.Run.performed -= Run_performed;
        actions.Run.canceled -= Run_canceled;
        actions.LockOn.performed -= LockOn_performed;
        actions.LockOn.canceled -= LockOn_canceled;
    }
    
    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(canDash)
        {
            float x = actions.Move.ReadValue<Vector2>().x;
            float z = actions.Move.ReadValue<Vector2>().y;
            if(isGrounded)
            {
                dashDirection = (transform.right * x) + (transform.forward * z);
                dashing = true;
            }
            else
            {
                if(airDashes < maxAirDashes)
                {
                    airDashes++;
                    dashDirection = (transform.right * x) + (camera.transform.forward * z);
                    dashing = true;
                }
            }
        }
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(canWallJump && !isWallJumping)
        {
            if(Physics.Raycast(transform.position, moveDirection, out RaycastHit hit,controller.radius + 0.5f, wallMask))
            {
                isWallJumping = true;
                wallJumpDirection = hit.normal + Vector3.up;
                wallJumpTimer = wallJumpTime;
            }
            else if (numberOfJumps < maxJumps)
            {
                velocity = Vector3.up * Mathf.Sqrt(jumpHeight * 2 * gravity);
                numberOfJumps++;
            }
        }
        else if (numberOfJumps < maxJumps)
        {
            velocity = Vector3.up * Mathf.Sqrt(jumpHeight * 2 * gravity);
            numberOfJumps++;
        }
    }

    private void Jump_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    private void Run_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        currentSpeed = runSpeed;
    }

    private void Run_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        currentSpeed = walkSpeed;
    }

    private void LockOn_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (Physics.Raycast(camera.position, camera.forward, out RaycastHit hit))
        {
            if (hit.transform.gameObject.layer == 6)
            {
                lockOnTarget = hit.transform;
            }
        }
    }

    private void LockOn_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        lockOnTarget = null;
    }

    void Dash()
    {
        if(dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
        }
        else
        {
            dashTimer = dashTime;
            dashing = false;
        }
    }
    
    void Movement()
    {
        if(isGrounded && velocity.y < 0)
        {
            velocity = Vector3.zero;
            numberOfJumps = 0;
            airDashes = 0;
        }

        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = transform.right * x + transform.forward * z;
        controller.Move(new Vector3(moveDirection.x, 0, moveDirection.z) * currentSpeed * Time.deltaTime);

        if(moveDirection.magnitude > 0) 
        {
            velocity.x = 0;
            velocity.z = 0;
        }

        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);


    }
    
    void LockOnMovement()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity = Vector3.zero;
            numberOfJumps = 0;
            airDashes = 0;
        }

        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = camera.transform.right * x + camera.transform.forward * z;
        controller.Move(new Vector3(moveDirection.x, 0, moveDirection.z) * currentSpeed * Time.deltaTime);


        //Add Gravity
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;
        //Looking up/down with camera
        xRot -= y * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90, 45);
        camera.localEulerAngles = new Vector3(xRot, 0, 0);
        //Looking left right with player body
        yRot += x * lookSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }

    void LookAtTarget()
    {
        Quaternion targetRot = Quaternion.LookRotation(lockOnTarget.position - camera.transform.position);
        xRot = targetRot.eulerAngles.x;
        yRot = targetRot.eulerAngles.y;
        camera.localEulerAngles = new Vector3(xRot, 0, 0);
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }

    void WallJump()
    {
        if(isWallJumping) 
        {
            if(wallJumpTimer > 0)
            {
                wallJumpTimer -= Time.deltaTime;
                Vector3 wallJumpForce = wallJumpDirection * Mathf.Sqrt(wallJumpPower * 2 * gravity);
                velocity = wallJumpForce;
            }
            else
            {
                isWallJumping = false;
            }
        }
            
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
