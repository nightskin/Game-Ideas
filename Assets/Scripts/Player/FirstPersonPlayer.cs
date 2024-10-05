using UnityEngine;

public class FirstPersonPlayer : MonoBehaviour
{
    //Components
    Controls controls;
    public Controls.PlayerActions actions;
    Transform camera;
    CharacterController controller;

    //For Basic Controls
    [SerializeField] float maxSpeed = 20;
    [SerializeField] float acceleration = 10; 
    float speed;
    [SerializeField][Min(1)] float drag = 5;
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

    //For ZipLatch
    public bool canZipLatch;
    [SerializeField] ParticleSystem boostEffect;
    [SerializeField] LineRenderer leftChain;
    [SerializeField] LineRenderer rightChain;
    [SerializeField] float zipSpeed = 50;
    

    bool zipping = false;
    RaycastHit latchTarget;

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
        actions.ZipLatch.performed += ZipLatch_performed;
        actions.ZipLatch.canceled += ZipLatch_canceled;
    }

    void Update()
    {
        Look();
        if(isWallRunning)
        {
            WallRunning();
        }
        else if(zipping)
        {
            ZipLatch();
        }
        else
        {
            Movement();
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(transform.position + (Vector3.down * controller.height / 2), controller.radius, jumpLayer);
        if (canWallRun) isAgainstWall = Physics.Raycast(transform.position, moveDirection, out wallHit, 1, jumpLayer);
    }

    void OnDestroy()
    {
        actions.Jump.performed -= Jump_performed;
        actions.ZipLatch.performed -= ZipLatch_performed;
        actions.ZipLatch.canceled -= ZipLatch_canceled;
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

    private void ZipLatch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(Physics.Raycast(camera.position, camera.forward, out latchTarget, 1000, jumpLayer))
        {
            zipping = true;
            boostEffect.Play();
            leftChain.gameObject.SetActive(true);
            rightChain.gameObject.SetActive(true);
        }
    }

    private void ZipLatch_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        zipping = false;
        boostEffect.Stop();
        leftChain.gameObject.SetActive(false);
        rightChain.gameObject.SetActive(false);
        velocity.y = 0;
    }

    void ZipLatch()
    {
        leftChain.SetPosition(0, leftChain.transform.position);
        leftChain.SetPosition(1, latchTarget.point);
        leftChain.textureScale = new Vector2(Vector3.Distance(latchTarget.point, leftChain.transform.position), 1);

        rightChain.SetPosition(0, rightChain.transform.position);
        rightChain.SetPosition(1, latchTarget.point);
        rightChain.textureScale = new Vector2(Vector3.Distance(latchTarget.point, rightChain.transform.position), 1);


        velocity = (latchTarget.point - transform.position).normalized * zipSpeed;
        controller.Move(velocity * Time.deltaTime);

        if (Vector3.Distance(transform.position, latchTarget.point) < controller.height)
        {
            zipping = false;
            boostEffect.Stop();
            leftChain.gameObject.SetActive(false);
            velocity.y = 0;
        }
    }

    void Movement()
    {
        zRot = Mathf.Lerp(zRot, 0, 10 * Time.deltaTime);

        if(isGrounded && velocity.y < 0) 
        {
            consecutiveJumpsMade = 0;
            velocity = Vector3.Lerp(velocity, Vector3.zero, drag * Time.deltaTime);
        }
        
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = (transform.right * x + transform.forward * z).normalized;

        if(moveDirection.magnitude > 0) 
        {
            if(isGrounded)
            {
                float y = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
                camera.localPosition = bobStartPosition + new Vector3(0, y, 0);
            }
            velocity.x = Mathf.Lerp(velocity.x, 0, drag * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, 0, drag * Time.deltaTime);
            speed = Mathf.Lerp(speed, maxSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            speed = Mathf.Lerp(speed, 0, drag * Time.deltaTime);
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
