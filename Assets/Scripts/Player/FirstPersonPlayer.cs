using UnityEngine;

public class FirstPersonPlayer : MonoBehaviour
{
    //Components
    [Header("Components")]
    Controls controls;
    public Controls.PlayerActions actions;
    public Camera camera;
    public CharacterController controller;

    //For Basic Controls
    [Header("General")]
    [SerializeField] float maxWalkSpeed = 20;
    [SerializeField] float acceleration = 10; 
    float currentSpeed;
    [SerializeField][Min(1)] float drag = 5;
    Vector3 moveDirection;
    public float lookSpeed = 100;

    float xRot = 0;
    float yRot = 0;
    float zRot = 0;

    //Camera Bob
    [Header("CameraBob")]
    [SerializeField] float bobAmplitude = 0.1f;
    [SerializeField] float bobFrequency = 0.1f;
    Vector3 bobStartPosition;

    //For Jumping and falling
    [Header("Jumping")]
    [SerializeField] int maxJumps = 2;
    [SerializeField] float jumpHeight = 3;
    [SerializeField] LayerMask jumpLayer;
    [SerializeField] float gravity = 10.0f;

    int consecutiveJumpsMade = 0;
    bool grounded = false;
    Vector3 velocity = Vector3.zero;

    //For Wall Movement
    [Header("Wall Running")]
    public bool canWallRun;
    [SerializeField] float wallRunSpeed = 30;
    [SerializeField] float wallRunAngle = 45;
    RaycastHit wallHit;
    [SerializeField] bool againstWall = false;
    bool wallRunning = false;

    //For ZipLatch
    [Header("ZipLatch")]
    public bool canZipLatch;
    [SerializeField] ParticleSystem boostEffect;
    [SerializeField] LineRenderer chain;
    [SerializeField] float zipSpeed = 50;

    float dramaticPause = 0.15f;
    bool zipping = false;
    RaycastHit latchTarget;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        camera = Camera.main;
        bobStartPosition = camera.transform.localPosition;

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

        if(wallRunning)
        {
            WallRunningMovement();
        }
        else
        {
            Movement();
        }
        
        if(zipping)
        {
            ZipLatch();
        }



    }

    void FixedUpdate()
    {
        grounded = Physics.CheckSphere(transform.position + (Vector3.down * controller.height / 2), controller.radius, jumpLayer);
        if (canWallRun) 
        {
            Ray right = new Ray(transform.position, transform.right);
            Ray left = new Ray(transform.position, -transform.right);

            Ray forwardRight = new Ray(transform.position, transform.right + transform.forward);
            Ray forwardLeft = new Ray(transform.position, (-transform.right) + transform.forward);

            againstWall = Physics.Raycast(right, out wallHit, 1, jumpLayer) || Physics.Raycast(left, out wallHit, 1, jumpLayer) || Physics.Raycast(forwardLeft, out wallHit, 1, jumpLayer) || Physics.Raycast(forwardRight, out wallHit, 1, jumpLayer);
        }
    }

    void OnDestroy()
    {
        actions.Jump.performed -= Jump_performed;
        actions.ZipLatch.performed -= ZipLatch_performed;
        actions.ZipLatch.canceled -= ZipLatch_canceled;
    }
    

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!grounded && againstWall)
        {
            if (!wallRunning)
            {
                wallRunning = true;
            }
            else
            {
                //Wall Jump
                consecutiveJumpsMade--;
                velocity = (Vector3.up + wallHit.normal) * Mathf.Sqrt(jumpHeight * 2 * gravity);
                wallRunning = false;
            }
        }
        if ((grounded || consecutiveJumpsMade < maxJumps) && !wallRunning)
        {
            //Jump Normally
            velocity = Vector3.up * Mathf.Sqrt(jumpHeight * 2 * gravity);
            consecutiveJumpsMade++;
        }
    }

    private void ZipLatch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(Physics.Raycast(camera.transform.position, camera.transform.forward, out latchTarget, camera.farClipPlane, jumpLayer))
        {
            zipping = true;
            dramaticPause = 0.15f;
            chain.gameObject.SetActive(true);
        }
    }

    private void ZipLatch_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        zipping = false;
        boostEffect.Stop();
        chain.gameObject.SetActive(false);
        velocity.y = 0;
    }

    void ZipLatch()
    {
        chain.SetPosition(0, chain.transform.position);
        chain.SetPosition(1, latchTarget.point);
        chain.textureScale = new Vector2(Vector3.Distance(latchTarget.point, chain.transform.position), 1);

        
        if (dramaticPause > 0)
        {
            dramaticPause -= Time.deltaTime;
        }
        else
        {
            if(!boostEffect.isEmitting) boostEffect.Play();
            velocity = (latchTarget.point - transform.position).normalized * zipSpeed;

            float x = actions.Move.ReadValue<Vector2>().x;
            float y = actions.Move.ReadValue<Vector2>().y;

            moveDirection = (transform.right * x + transform.up * y).normalized;
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        }

        controller.Move(velocity * Time.deltaTime);
        if (Vector3.Distance(transform.position, latchTarget.point) < controller.height)
        {
            boostEffect.Stop();
            chain.gameObject.SetActive(false);
            velocity = Vector3.zero;
        }

    }

    void Movement()
    {
        zRot = Mathf.Lerp(zRot, 0, 10 * Time.deltaTime);

        if(grounded && velocity.y < 0) 
        {
            consecutiveJumpsMade = 0;
            velocity = Vector3.Lerp(velocity, Vector3.zero, drag * Time.deltaTime);
        }
        
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = (transform.right * x + transform.forward * z).normalized;

        if(moveDirection.magnitude > 0) 
        {
            if(grounded)
            {
                float y = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
                camera.transform.localPosition = bobStartPosition + new Vector3(0, y, 0);
            }
            if (Vector3.Dot(velocity.normalized, moveDirection) < 0)
            {
                velocity.x = Mathf.Lerp(velocity.x, 0, drag * Time.deltaTime);
                velocity.z = Mathf.Lerp(velocity.z, 0, drag * Time.deltaTime);
            }
            currentSpeed = Mathf.Lerp(currentSpeed, maxWalkSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, drag * Time.deltaTime);
        }

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);




        velocity += Vector3.down * gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    void WallRunningMovement()
    {
        moveDirection = -wallHit.normal;

        if ((transform.right + wallHit.normal).magnitude > (-transform.right + wallHit.normal).magnitude)
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
        controller.Move((wallForward + new Vector3(0, camera.transform.forward.y, 0)) * wallRunSpeed * Time.deltaTime);


        float y = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        camera.transform.localPosition = bobStartPosition + new Vector3(0, y, 0);

        if (!againstWall)
        {
            wallRunning = false;
        }

    }

    void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;

        //Looking up/down with camera
        xRot -= y * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90, 90);
        camera.transform.localEulerAngles = new Vector3(xRot, 0, zRot);
        
        //Looking left right with player body
        yRot += x * lookSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);

    }
    
}
