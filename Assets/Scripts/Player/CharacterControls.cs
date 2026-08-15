using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterControls : MonoBehaviour
{
    [Header("Components")]
    public Transform camera;
    public CharacterController controller;
    public Animator animator;
    

    [Header("Movement")]
    public float jumpHeight = 1;
    public Vector3 velocity = Vector3.zero;
    public float speed = 20;
    float currentMoveSpeed;
    bool jumping = false;

    [Header("Looking")]
    [Range(0,90)] public float maxLookY = 45;
    [SerializeField] float cameraBobSpeed = 5;
    [SerializeField] float cameraBobHeight = 0.25f;
    float rotx = 0;
    float roty = 0;

    [Header("Lock On System")]
    [SerializeField] float lockOnDistance = 50;
    [SerializeField] LayerMask lockOnLayerMask;
    [SerializeField] Transform lockOnTarget = null;
    float lockOnLerp = 0;

    [Header("Physics")]
    public bool gravityOn = true; 
    public float gravityStrength = 10;
    [SerializeField][Min(0)] float groundDistance = 0.5f;
    [HideInInspector] public bool grounded;

    [Header("Dashing")]
    [SerializeField] float dashTime = 0.1f;
    [SerializeField] float dashSpeed = 250;
    Vector2 prevDashInput;
    float maxKeybaordPressTime = 0.25f; 
    float keyboardPressTime = 0;
    bool dashing = false;

    [Header("Combat")]
    [SerializeField] Transform armPivot;
    float cutAngle = 0;
    Vector2 actionVector = Vector2.zero;
    public enum State
    {
        IDLE,
        ATTACKING,
        DEFENDING,
    }
    State state;

    //Events
    void Start()
    {
        currentMoveSpeed = speed;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if(Game.controls.Player.LockOn.WasPerformedThisFrame())
        {
            if(lockOnTarget)
            {
                lockOnTarget = null;
            }
            else
            {
                if(Physics.Raycast(camera.transform.position,camera.transform.forward,out RaycastHit hit,lockOnDistance,lockOnLayerMask))
                {
                    lockOnTarget = hit.transform;
                }
            }

        }


        if(lockOnTarget)
        {
            LockOn();
        }
        else
        {
            FreeLook();
        }

        Movement();
        Combat();
    }

    void FixedUpdate()
    {
        // Checks If player is grounded
        Ray groundRay = new Ray(transform.position, Vector3.down);
        grounded = Physics.Raycast(groundRay, out RaycastHit hit, groundDistance);
    }

    //Functions
    void LockOn()
    {
        Vector3 lookDirection = lockOnTarget.transform.position - camera.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);

        
        if(lockOnLerp < 1) lockOnLerp += Time.deltaTime;
        lockOnLerp = Mathf.Clamp01(lockOnLerp);

        rotx = Mathf.LerpAngle(rotx, lookRotation.eulerAngles.x, lockOnLerp);
        roty = Mathf.LerpAngle(roty, lookRotation.eulerAngles.y, lockOnLerp);

        camera.transform.localEulerAngles = new Vector3(rotx,0,0);
        transform.localEulerAngles = new Vector3(0,roty,0);


        if(Vector3.Distance(transform.position, lockOnTarget.position) > lockOnDistance)
        {
            lockOnLerp = 0;
            lockOnTarget = null;
        }
    }
    
    bool DashKeyboardInput()
    {
        if(Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            return false;
        }
        else if(Keyboard.current.aKey.isPressed || Keyboard.current.dKey.isPressed || Keyboard.current.wKey.isPressed || Keyboard.current.sKey.isPressed)
        {
            keyboardPressTime += Time.deltaTime;
            return false;
        }
        else if(Keyboard.current.aKey.wasReleasedThisFrame || Keyboard.current.dKey.wasReleasedThisFrame || Keyboard.current.wKey.wasReleasedThisFrame || Keyboard.current.sKey.wasReleasedThisFrame)
        {
            if(keyboardPressTime < maxKeybaordPressTime)
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
        Vector2 lookInput = Game.controls.Player.Look.ReadValue<Vector2>();
        rotx -= lookInput.y * Game.aimSense * Time.deltaTime;
        rotx = Mathf.Clamp(rotx,-maxLookY,maxLookY);
        roty += lookInput.x * Game.aimSense * Time.deltaTime;
        camera.transform.localEulerAngles = new Vector3(rotx,0,0);
        transform.localEulerAngles = new Vector3(0,roty,0);
    }

    void Movement()
    {
        // When player hits the ground
        if (grounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        // Moving Around 
        float x = Game.controls.Player.Move.ReadValue<Vector2>().x;
        if(lockOnTarget)
        {
            float normalizeDistance = Game.Remap(Vector3.Distance(transform.position, lockOnTarget.position),0,lockOnDistance, 0,1);
            float invertedDistance = Game.InvertRange(normalizeDistance,0,1);
            x *= invertedDistance;
        }
        float z = Game.controls.Player.Move.ReadValue<Vector2>().y;
        float m = Game.controls.Player.Move.ReadValue<Vector2>().magnitude;
        Vector3 moveDirection = (transform.right * x + transform.forward * z).normalized * m;

        //dashing input for some reason interactions in input asset does not work 
        if(DashKeyboardInput() && !dashing)
        {
            if(lockOnTarget)
            {
                StartCoroutine(Dash(prevDashInput, true));
            }
            else
            {
                StartCoroutine(Dash(prevDashInput, false));
            }
        }
        else if(Gamepad.current.leftShoulder.isPressed && !dashing)
        {
            if(lockOnTarget)
            {
                StartCoroutine(Dash(new Vector2(x,z), true));
            }
            else
            {
                StartCoroutine(Dash(prevDashInput, false));
            }
        }

        if(!dashing)
        {
            controller.Move(moveDirection * currentMoveSpeed * Time.deltaTime);
        }
        
        //Fixes Moving Down Slopes
        if(grounded && moveDirection.magnitude > 0 && !jumping)
        {
            Physics.Raycast(transform.position,Vector3.down,out RaycastHit hit,groundDistance);
            controller.Move(Vector3.down * hit.distance);
        }

        prevDashInput = new Vector2(x,z);

        //Gravity
        if(gravityOn) velocity.y -= gravityStrength * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //Jumping
        if(Game.controls.Player.Jump.WasPerformedThisFrame() && grounded)
        {
            StartCoroutine(Jump());
        }
    }

    void Combat()
    {
        actionVector = Game.controls.Player.Look.ReadValue<Vector2>();

        if(state != State.IDLE)
        {

        }

        if(Game.controls.Player.Attack.WasPerformedThisFrame())
        {
            cutAngle = Mathf.Atan2(actionVector.x, -actionVector.y) * 180 / Mathf.PI;
            animator.SetTrigger("cut");
        }
        else if(Game.controls.Player.Defend.IsPressed())
        {
            
        }
    }

    IEnumerator Jump()
    {
        jumping = true;
        velocity.y = Mathf.Sqrt(jumpHeight * 2 * gravityStrength);
        yield return new WaitForSeconds(0.2f);
        jumping = false;
    }

    IEnumerator Dash(Vector2 dashInput,bool lockedOn)
    {
        dashing = true;
        float t = 0;
        Vector3 dashDirection = (transform.right * dashInput.x + transform.forward * dashInput.y).normalized;
        if(lockedOn)
        {
            float normalizeDistance = Game.Remap(Vector3.Distance(transform.position, lockOnTarget.position),0,lockOnDistance, 0,1);
            float invertedDistance = Game.InvertRange(normalizeDistance,0,1);
            dashInput.x = dashInput.x * invertedDistance;
        }

        while(t < dashTime)
        {
            if(lockedOn)
            {
                dashDirection = (transform.right * dashInput.x + transform.forward * dashInput.y).normalized;
            }
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }
        dashing = false;
    }
    //Animation Events
    public void AttackState()
    {
        armPivot.localEulerAngles = new Vector3(0,0,cutAngle);
        state = State.ATTACKING;
    }
    
    public void DefendState()
    {
        armPivot.localEulerAngles = Vector3.zero;
        state = State.DEFENDING;
    }
    
    public void IdleState()
    {
        armPivot.localEulerAngles = Vector3.zero;
        state = State.IDLE;
    }

}
