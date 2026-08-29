using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class Player : MonoBehaviour
{
    [Header("Components")]
    public Transform cameraHolder;
    public CharacterController controller;
    public Image reticle;
    public Animator animator;
    public Transform armPivot;


    [Header("Movement")]
    [Min(0)] public int maxNumberOfJumps = 1;
    [Min(1)] public float jumpHeight = 1;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [Min(1)] public float normalSpeed = 20;
    [Min(1)] public float dashSpeed = 150;
    [Min(1)] public float crouchSpeed = 10;
    [HideInInspector] public float targetSpeed = 0;
    [HideInInspector] public float currentSpeed = 0;
    bool crouching = false;
    bool jumping = false;
    bool isEvading = false;
    int jumpsTaken = 0;
    [SerializeField] float dashTime = 0.1f;


    [Header("Looking Around")]
    [Range(0,90)] public float maxLookY = 45;
    [HideInInspector] public float lookSpeed;
    [HideInInspector] public float rotx = 0;
    [HideInInspector] public float roty = 0;

    [Header("Physics")]
    public bool gravityOn = true; 
    public float gravityStrength = 10;
    [SerializeField][Min(0)] float groundDistance = 0.5f;
    [HideInInspector] public bool onGround;

    [Header("Combat")]
    Vector2 atkVector;
    float atkAngle = 0;
    [HideInInspector] public bool isAttacking = false;

    [Header("Wall Movement")]
    [SerializeField] bool enableWallRun = true;
    [SerializeField][Range(0,90)] float cameraTiltAngleWhileWallRunning = 35; 
    [SerializeField] float cameraTiltSpeed = 10;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] bool allowPlayerToJumpAgainAfterWallRun = true;
    [SerializeField] float wallDistance = 1;
    RaycastHit wallHit;
    float cameraTilt = 0;
    bool canTakenExtraJump;
    bool isWallRunning = false;
    bool canWallRun = false;
    bool canWallJump = false;

    [Header("GroundSlam")]
    [SerializeField] bool groundSlamEnabled = true;

    [Header("Homing Dash")]
    [SerializeField] bool homingDashEnabled = true;
    [SerializeField][Min(10)] float maxHomingDistance = 100;
    [SerializeField][Min(0)] float atkDistance = 2;
    bool isDashing = false;

    //Extra variables
    Vector2 prevMoveInput;
    float maxKeyboardPressTime = 0.25f; 
    float keyboardPressTime = 0;


    //Events
    void Start()
    {
        if(Physics.Raycast(transform.position,Vector3.down,out RaycastHit hit))
        {
            transform.position = hit.point;
        }
        lookSpeed = Game.settings.aimSense;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Combat();
        FreeLook();
        ApplyPhysics();
        if(homingDashEnabled)
        {
            if(Game.input.Player.Dash.WasPerformedThisFrame())
            {
                if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, maxHomingDistance))
                {
                    StartCoroutine(HomingDash(hit.point));
                }
            }
        }
        if(enableWallRun) WallMovement();
        if(!isEvading && !isDashing) NormalMovement();
        Evasion();
    }

    void FixedUpdate()
    {
        currentSpeed = Mathf.Lerp(currentSpeed,targetSpeed, 10 * Time.deltaTime);

        if(enableWallRun)
        {
            Ray rayleft = new Ray(transform.position, -transform.right);
            Ray rayRight = new Ray(transform.position, transform.right);
            canWallRun = Physics.Raycast(rayleft, out wallHit, wallDistance,wallLayer) || Physics.Raycast(rayRight, out wallHit, wallDistance, wallLayer);
        }

        // Checks If player is grounded
        Ray groundRay = new Ray(transform.position, Vector3.down);
        onGround = Physics.Raycast(groundRay, out RaycastHit hit, groundDistance);
    }

    //Helper Functions
    void Combat()
    {
        atkVector = Game.input.Player.Look.ReadValue<Vector2>();
        atkAngle = Mathf.Atan2(atkVector.x, -atkVector.y) * Mathf.Rad2Deg;
        atkAngle = Mathf.Clamp(atkAngle,-135,135);
        //reticle.rectTransform.rotation = Quaternion.Euler(0,0,atkAngle);
        
        if(Game.input.Player.Attack.WasPerformedThisFrame())
        {   
            animator.SetTrigger("atk");
        }
    }
    // built-in tapping checks for some reason does not work so I had to implement my own
    bool EvadeKeyboardInput()
    {
        if(Game.input.Player.EvadeK.WasPerformedThisFrame())
        {
            return false;
        }
        else if(Game.input.Player.EvadeK.IsPressed())
        {
            keyboardPressTime += Time.deltaTime;
            return false;
        }
        else if(Game.input.Player.EvadeK.WasReleasedThisFrame())
        {
            if(keyboardPressTime < maxKeyboardPressTime)
            {
                keyboardPressTime = 0;
                return true;
            }
            else
            {
                keyboardPressTime = 0;
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    
    void FreeLook()
    {
        //Looking Around
        Vector2 lookInput = Game.input.Player.Look.ReadValue<Vector2>();
        rotx -= lookInput.y * lookSpeed * Time.deltaTime;
        rotx = Mathf.Clamp(rotx,-maxLookY,maxLookY);
        roty += lookInput.x * lookSpeed * Time.deltaTime;
        cameraHolder.transform.localEulerAngles = new Vector3(rotx,0,0);
        transform.localEulerAngles = new Vector3(0,roty,0);
    }
    
    void ApplyPhysics()
    {
        // When player hits the ground
        if (onGround && velocity.y < 0)
        {
            velocity = Vector3.zero;
            jumpsTaken = 0;
        }

        //Gravity
        if(gravityOn) velocity.y -= gravityStrength * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //fixes falling down slopes issue
        if(onGround && Game.input.Player.Move.IsPressed() && !jumping)
        {
            Physics.Raycast(transform.position,Vector3.down,out RaycastHit hit,groundDistance);
            controller.Move(Vector3.down * hit.distance);
        }
    }

    void NormalMovement()
    {
        float xMoveInput = Game.input.Player.Move.ReadValue<Vector2>().x;
        float zMoveInput = Game.input.Player.Move.ReadValue<Vector2>().y;
        float magnitude = Game.input.Player.Move.ReadValue<Vector2>().magnitude;
        Vector3 moveDirection = (transform.right * xMoveInput + transform.forward * zMoveInput).normalized * magnitude;

        if(Game.input.Player.Move.IsPressed())
        {
            if(Game.settings.cameraBob && onGround)
            {
                float offset = Mathf.Sin(Time.time * Game.settings.cameraBobSpeed) * Game.settings.cameraBobMaxHeight;
                Camera.main.transform.localPosition = Camera.main.transform.up * offset;
            }

            targetSpeed = normalSpeed;
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
        else
        {
            targetSpeed = 0;
        }

        if(Game.input.Player.Jump.WasPerformedThisFrame() && jumpsTaken < maxNumberOfJumps)
        {
            StartCoroutine(Jump());
        }
        
        if(Game.input.Player.Crouch.WasPerformedThisFrame() && onGround)
        {
            Crouch();
        }
        else if(Game.input.Player.Crouch.IsPressed() && !onGround)
        {
            controller.Move(Vector3.down * currentSpeed * Time.deltaTime);
        }
    }
    
    void Evasion()
    {
        if(onGround)
        {
            Vector2 moveInput = Game.input.Player.Move.ReadValue<Vector2>();
            if(EvadeKeyboardInput() && !isEvading)
            {
                StartCoroutine(Evade(prevMoveInput));
            }
            else if(Game.input.Player.EvadeG.WasPerformedThisFrame() && !isEvading)
            {
                StartCoroutine(Evade(moveInput));
            }

            prevMoveInput = moveInput;
        }

    }

    void WallMovement()
    {
        //Conditions for Wall Run
        if(canWallRun && !isWallRunning && !onGround && Game.input.Player.Jump.WasPerformedThisFrame())
        {
            StartWallRun();
        }

        if(isWallRunning)
        {
            //Wall Running Movement
            Vector3 wallNormal = wallHit.normal;
            Vector3 wallForward = Vector3.Cross(wallNormal,transform.up);
            if(Vector3.Dot(transform.forward,wallForward) < 0)
            {
                wallForward = -wallForward;
            }
            controller.Move((wallForward + new Vector3(0,cameraHolder.forward.y,0)).normalized * currentSpeed * Time.deltaTime);

            //Tilt Camera
            if(Vector3.Dot(wallNormal,transform.right) > Vector3.Dot(wallNormal,-transform.right))
            {
                cameraTilt = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, -cameraTiltAngleWhileWallRunning, cameraTiltSpeed * Time.deltaTime);
            }
            else
            {
                cameraTilt = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, cameraTiltAngleWhileWallRunning, cameraTiltSpeed * Time.deltaTime);
            }

            //Wall Jumping
            if(Game.input.Player.Jump.WasPerformedThisFrame() && canWallJump)
            {
                EndWallRun();
                velocity = (wallHit.normal + Vector3.up).normalized * Mathf.Sqrt(jumpHeight * 2 * 10);
            }
            //Cancel Wall Run
            if(!canWallRun || onGround)
            {
                EndWallRun();
            }
            
            canWallJump = true;
        }
        else
        {
            if(Game.input.Player.Move.ReadValue<Vector2>().magnitude > 0.5f)
            {
                StartCoroutine(ApplyDrag(1,0));
            }
            if(canTakenExtraJump && Game.input.Player.Jump.WasPerformedThisFrame() && allowPlayerToJumpAgainAfterWallRun)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * 2 * gravityStrength);
                canTakenExtraJump = false;
            }
            cameraTilt = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, 0, cameraTiltSpeed * Time.deltaTime);
        }

        Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, cameraTilt);
    }

    void Crouch()
    {
        if(!crouching)
        {
            targetSpeed = crouchSpeed;
            cameraHolder.transform.localPosition = new Vector3(0,1.25f,0);
            controller.height = 1.25f;
            crouching = true;
        }
        else
        {
            targetSpeed = normalSpeed;
            cameraHolder.transform.localPosition = new Vector3(0,2,0);
            controller.height = 2f;
            crouching = false;
        }
    }
    IEnumerator HomingDash(Vector3 point)
    {
        isDashing = true;
        while(Vector3.Distance(transform.position, point) <= atkDistance)
        {
            transform.position = Vector3.Lerp(transform.position, point, 10 * Time.deltaTime);
            yield return null;
        }
        isDashing = false;
    }
    IEnumerator Evade(Vector2 dashInput)
    {
        isEvading = true;
        float t = 0;
        Vector3 dashDirection = (transform.right * dashInput.x + transform.forward * dashInput.y).normalized;

        while(t < dashTime)
        {
            targetSpeed = dashSpeed;
            controller.Move(dashDirection * currentSpeed * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
        targetSpeed = normalSpeed;
        isEvading = false;
    }
    IEnumerator Jump()
    {
        jumping = true;
        velocity.y = Mathf.Sqrt(jumpHeight * 2 * gravityStrength);
        jumpsTaken++;
        yield return new WaitForSeconds(0.25f);
        jumping = false;
    }
    IEnumerator ApplyDrag(float amount = 1 , float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        float t = 1;
        while(t > 0)
        {
            velocity = Vector3.Lerp(velocity, new Vector3(0, velocity.y, 0),t);
            t -= amount * Time.deltaTime;
            yield return null;
        }
    }
    
    void StartWallRun()
    {
        canTakenExtraJump = true;
        canWallJump = false;
        gravityOn = false;
        isWallRunning = true;
    }
    void EndWallRun()
    {
        gravityOn = true;
        isWallRunning = false;
    }

    //Animation events
    public void StartAttack()
    {
        armPivot.localEulerAngles = new Vector3(0,0,atkAngle);
        isAttacking = true;
    }
    public void EndAttack()
    {
        armPivot.localEulerAngles = Vector3.zero;
        isAttacking = false;
    }
}
