using UnityEngine;

public class FirstPersonPlayer : MonoBehaviour
{
    //Components
    Controls controls;
    public Controls.PlayerActions actions;
    Transform camera;
    CharacterController controller;

    //For Basic Controls
    float speed = 20;
    Vector3 moveDirection;
    public float lookSpeed = 100;

    float xRot = 0;
    float yRot = 0;
    float zRot = 0;

    //Camera Bob
    [SerializeField] float bobAmplitude = 0.1f;
    [SerializeField] float bobFrequency = 0.1f;
    Vector3 bobStartPosition;

    //For Jumping and falling
    [SerializeField] int maxJumps = 2;
    [SerializeField] float jumpHeight = 3;
    [SerializeField] LayerMask jumpLayer;
    [SerializeField] float gravity = 10.0f;

    int consecutiveJumpsMade = 0;
    bool isGrounded = false;
    Vector3 velocity = Vector3.zero;

    //For Wall Movement
    public bool canWallRun;
    [SerializeField] float wallRunAngle = 45;
    RaycastHit wallHit;
    bool isAgainstWall = false;
    bool isWallRunning = false;
    
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        camera = transform.Find("Camera");
        bobStartPosition = camera.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();

        actions.Jump.performed += Jump_performed;
        actions.Jump.canceled += Jump_canceled;
    }

    void Update()
    {
        Look();
        if(isWallRunning)
        {
            WallRunning();
        }
        else
        {
            Movement();
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(transform.position + (Vector3.down * controller.height / 2), controller.radius, jumpLayer);
        if(canWallRun) isAgainstWall = Physics.Raycast(transform.position, transform.right, out wallHit, 1, jumpLayer) || Physics.Raycast(transform.position, -transform.right, out wallHit, 1, jumpLayer);
    }

    void OnDestroy()
    {
        actions.Jump.performed -= Jump_performed;
        actions.Jump.canceled -= Jump_canceled;
    }
    
    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if ((isGrounded || consecutiveJumpsMade < maxJumps) && !isWallRunning)
        {
            //Jump Normally
            velocity = Vector3.up * Mathf.Sqrt(jumpHeight * 2 * gravity);
            consecutiveJumpsMade++;
        }
        if(!isGrounded && isAgainstWall)
        {
            if(!isWallRunning)
            {
                isWallRunning = true;
            }
            else
            {
                //Wall Jump
                consecutiveJumpsMade--;
                velocity = (Vector3.up + wallHit.normal) * Mathf.Sqrt(jumpHeight * 2 * gravity);
                isWallRunning = false;
            }
        }
    }

    private void Jump_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }
    
    void Movement()
    {
        zRot = Mathf.Lerp(zRot, 0, 10 * Time.deltaTime);

        if(isGrounded && (velocity.y < 0 || velocity.x != 0 || velocity.z != 0)) 
        {
            consecutiveJumpsMade = 0;
            velocity = Vector3.zero;
        }
        
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = (transform.right * x + transform.forward * z).normalized;

        if(moveDirection.magnitude > 0 && isGrounded) 
        {
            float y = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            camera.localPosition = bobStartPosition + new Vector3(0, y, 0);
        }

        controller.Move(moveDirection * speed * Time.deltaTime);

        velocity += Vector3.down * gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    void WallRunning()
    {
        if((transform.right + wallHit.normal).magnitude > (-transform.right + wallHit.normal).magnitude)
        {
            zRot = Mathf.Lerp(zRot, -wallRunAngle, 10 * Time.deltaTime);
        }
        else if ((transform.right + wallHit.normal).magnitude < (-transform.right + wallHit.normal).magnitude)
        {
            zRot = Mathf.Lerp(zRot, wallRunAngle, 10 * Time.deltaTime);
        }

        Vector3 wallForward = Vector3.Cross(wallHit.normal, transform.up);
        if((transform.forward - wallForward).magnitude > (transform.forward + wallForward).magnitude)
        {
            wallForward = -wallForward;
        }
        controller.Move((wallForward + new Vector3(0, camera.forward.y, 0)) * speed * Time.deltaTime);

        float y = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        camera.localPosition = bobStartPosition + new Vector3(0, y, 0);

        if (!isAgainstWall)
        {
            isWallRunning = false;
        }

    }

    void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;

        //Looking up/down with camera
        xRot -= y * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -45, 45);
        camera.localEulerAngles = new Vector3(xRot, 0, zRot);
        
        //Looking left right with player body
        yRot += x * lookSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);

    }
    
}
