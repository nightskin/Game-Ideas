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

    float lookSpeed = 100;
    float xRot = 0;
    float yRot = 0;

    // For Jumping
    float jumpHeight = 5;
    Vector3 velocity = Vector3.zero;
    [SerializeField] LayerMask groundMask;
    Transform groundCheck;
    float gravity = -9.81f;
    public bool gravityOn = true;
    public bool isGrounded = false;

    
    //For lockOn System
    Transform lockOnTarget = null;

    //For Wall Jumping and Wall Running
    bool isWallRunning = false;
    float wallDistance = 1.0f;
    bool isAgainstWallLeft = false;
    bool isAgainstWallRight = false;
    RaycastHit wallHitLeft;
    RaycastHit wallHitRight;

    void Awake()
    {
        currentSpeed = walkSpeed;
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
        }
        else if (lockOnTarget)
        {
            Quaternion targetRot = Quaternion.LookRotation(lockOnTarget.position - camera.transform.position);
            xRot = targetRot.eulerAngles.x;
            yRot = targetRot.eulerAngles.y;
            camera.localEulerAngles = new Vector3(xRot, 0, 0);
            transform.localEulerAngles = new Vector3(0, yRot, 0);
        }

        if (isWallRunning) WallRunningMovement();
        Movement();

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
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
        if(isWallRunning)
        {
            Vector3 wallNormal = isAgainstWallRight ? wallHitRight.normal : wallHitLeft.normal;
            velocity = (wallNormal + transform.up).normalized;
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
        isAgainstWallRight = Physics.Raycast(transform.position, transform.right, out wallHitRight, wallDistance, groundMask);
        isAgainstWallLeft = Physics.Raycast(transform.position, -transform.right, out wallHitLeft, wallDistance, groundMask);
    }

    void WallRunInput()
    {
        if((isAgainstWallLeft || isAgainstWallRight) && actions.Move.ReadValue<Vector2>().y > 0 && !isGrounded)
        {
            isWallRunning = true;
            gravityOn = false;
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
        camera.localEulerAngles = new Vector3(xRot, 0, 0);
        //Looking left right with player body
        yRot += x * lookSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }

    void CanJump()
    {
        if(gravityOn) isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, 0.25f, groundMask);
        
    }
    
}
