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
    float zRot = 0;

    // For Jumping
    float jumpHeight = 5;
    Vector3 velocity = Vector3.zero;
    Transform groundCheck;
    float gravity = -9.81f;
    public bool gravityOn = true;
    public bool isGrounded = false;

    
    //For lockOn System
    Transform lockOnTarget = null;

    //For Wall Jumping and Wall Running
    [SerializeField] LayerMask wallMask;
    [SerializeField] float maxWallRunTime = 5;
    float wallJumpSideForce = 15;
    float wallJumpUpForce = 5;
    float maxWallRunTimer = 0;
    
    bool isExitingWall = false;
    float exitingWallTime = 0.25f;
    float exitWallTimer = 0;

    bool isWallRunning = false;
    float wallDistance = 0.6f;
    bool isAgainstWallLeft = false;
    bool isAgainstWallRight = false;
    RaycastHit wallHitLeft;
    RaycastHit wallHitRight;

    void Awake()
    {
        currentSpeed = walkSpeed;
        if (!camera) camera = transform.Find("Camera");
        if(!groundCheck) groundCheck = transform.Find("GroundCheck");
        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();


        actions.Jump.performed += Jump_performed;
        actions.Run.performed += Run_performed;
        actions.Run.canceled += Run_canceled;
        actions.LockOn.performed += LockOn_performed;
        actions.LockOn.canceled += LockOn_canceled;
    }
    
    void Update()
    {
        CanJump();
        CheckWall();
        WallRunInput();

        if (!lockOnTarget)
        {
            Look();
            Movement();
        }
        else if (lockOnTarget)
        {
            LookAtTarget();
            LockOnMovement();
        }

        if (isWallRunning && !isExitingWall) 
        {
            WallRunningMovement();
        }
        else
        {
            zRot = Mathf.Lerp(zRot, 0, 10 * Time.deltaTime);
        }

    }
    
    void OnDestroy()
    {
        actions.Jump.performed -= Jump_performed;
        actions.Run.performed -= Run_performed;
        actions.Run.canceled -= Run_canceled;
        actions.LockOn.performed -= LockOn_performed;
        actions.LockOn.canceled -= LockOn_canceled;
    }


    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        maxWallRunTimer = maxWallRunTime;
        zRot = 0;
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
        if(isWallRunning)
        {
            isExitingWall = true;
            exitWallTimer = exitingWallTime;
            Vector3 wallNormal = isAgainstWallRight ? wallHitRight.normal : wallHitLeft.normal;
            Vector3 jumpForce = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
            velocity = jumpForce;
        }
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

    void CheckWall()
    {
        isAgainstWallRight = Physics.Raycast(transform.position, transform.right, out wallHitRight, wallDistance, wallMask);
        isAgainstWallLeft = Physics.Raycast(transform.position, -transform.right, out wallHitLeft, wallDistance, wallMask);
    }

    void WallRunInput()
    {
        if((isAgainstWallLeft || isAgainstWallRight) && actions.Move.ReadValue<Vector2>().y > 0 && !isGrounded && !isExitingWall)
        {
            isWallRunning = true;
            gravityOn = false;

            if(maxWallRunTimer > 0)
            {
                maxWallRunTimer -= Time.deltaTime;
            }
            else
            {
                isExitingWall = true;
                exitWallTimer = exitingWallTime;
            }
        }
        else if(isExitingWall)
        {
            if(isWallRunning)
            {
                isWallRunning = false;
                gravityOn = true;
            }

            if(exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }
            else
            {
                isExitingWall = false;
            }
        }
        else
        {
            isWallRunning = false;
            gravityOn = true;
        }
    }

    void WallRunningMovement()
    {
        Vector3 wallNormal = isAgainstWallRight ? wallHitRight.normal : wallHitLeft.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        float targetCameraAngle = isAgainstWallRight ? 30 : -30f;
        zRot = Mathf.LerpAngle(zRot, targetCameraAngle, 10 * Time.deltaTime);
        camera.localEulerAngles = new Vector3(xRot, 0, zRot);
        
        if((transform.forward - wallForward).magnitude > (transform.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        controller.Move(wallForward * currentSpeed * Time.deltaTime);
    }

    void Movement()
    {
        if(isGrounded && velocity.y < 0)
        {
            velocity = Vector3.zero;
            maxWallRunTimer = maxWallRunTime;
            zRot = 0;
        }

        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = transform.right * x + transform.forward * z;
        controller.Move(new Vector3(moveDirection.x, 0, moveDirection.z) * currentSpeed * Time.deltaTime);


        //Add Gravity
        if (gravityOn)
        {
            velocity += Vector3.up * gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }


    }
    
    void LockOnMovement()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity = Vector3.zero;
            maxWallRunTimer = maxWallRunTime;
            zRot = 0;
        }

        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = camera.transform.right * x + camera.transform.forward * z;
        controller.Move(new Vector3(moveDirection.x, 0, moveDirection.z) * currentSpeed * Time.deltaTime);


        //Add Gravity
        if (gravityOn)
        {
            velocity += Vector3.up * gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }

    void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;
        //Looking up/down with camera
        xRot -= y * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90, 45);
        camera.localEulerAngles = new Vector3(xRot, 0, zRot);
        //Looking left right with player body
        yRot += x * lookSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }

    void LookAtTarget()
    {
        Quaternion targetRot = Quaternion.LookRotation(lockOnTarget.position - camera.transform.position);
        xRot = targetRot.eulerAngles.x;
        yRot = targetRot.eulerAngles.y;
        camera.localEulerAngles = new Vector3(xRot, 0, zRot);
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }

    void CanJump()
    {
        if(gravityOn) isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, 0.25f);
    }
    
}
