using UnityEngine;

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
    Vector3 latchOffset;

    void Awake()
    {
        combatControls = GetComponent<PlayerCombatControls>();
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
        Movement();
        if (zipping)
        {
            ZipLatch();
        }
    }

    void FixedUpdate()
    {
        grounded = Physics.CheckSphere(transform.position + (Vector3.down * controller.height / 2), controller.radius, jumpLayer);
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
            if(!latchTarget.collider.isTrigger)
            {
                zipping = true;
                dramaticPause = 0.15f;
                chain.gameObject.SetActive(true);
                latchOffset = latchTarget.point - latchTarget.transform.position;
            }
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
        Vector3 latchPoint;
        if(latchTarget.transform.gameObject.isStatic)
        {
            latchPoint = latchTarget.point;
        }
        else
        {
            latchPoint = latchTarget.transform.position + latchOffset;
        }

        chain.SetPosition(0, chain.transform.position);
        chain.SetPosition(1, latchPoint);
        chain.textureScale = new Vector2(Vector3.Distance(latchPoint, chain.transform.position), 1);

        
        if (dramaticPause > 0)
        {
            dramaticPause -= Time.deltaTime;
        }
        else
        {
            if(!boostEffect.isEmitting) boostEffect.Play();
            velocity = (latchPoint - transform.position).normalized * zipSpeed;

            float x = actions.Move.ReadValue<Vector2>().x;
            float y = actions.Move.ReadValue<Vector2>().y;

            moveDirection = (transform.right * x + transform.up * y).normalized;
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        }

        controller.Move(velocity * Time.deltaTime);
        if (Vector3.Distance(transform.position, latchPoint) < controller.height)
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
