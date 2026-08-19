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
    [HideInInspector] public bool onGround;

    [Header("Dashing")]
    [SerializeField] float dashTime = 0.1f;
    [SerializeField] float dashSpeed = 250;
    Vector2 prevDashInput;
    float maxKeybaordPressTime = 0.25f; 
    float keyboardPressTime = 0;
    bool dashing = false;

    [Header("Combat")]
    [SerializeField] Transform armPivot;
    float atkAngle = 0;
    Vector2 defAngle = Vector2.zero;
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
        if(Game.input.Player.LockOn.WasPerformedThisFrame())
        {
            if(lockOnTarget)
            {
                LockOff();
            }
            else
            {
                LockOn();
            }
        }


        if(lockOnTarget)
        {
            ToTowardsTarget();
        }
        else
        {
            FreeLook();
        }

        ApplyPhysics();
        MovementControls();
        CombatControls();
    }

    void FixedUpdate()
    {
        // Checks If player is grounded
        Ray groundRay = new Ray(transform.position, Vector3.down);
        onGround = Physics.Raycast(groundRay, out RaycastHit hit, groundDistance);
    }

    //Helper Functions
    void ToTowardsTarget()
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
    
    void FreeLook()
    {
        //Looking Around
        Vector2 lookInput = Game.input.Player.Look.ReadValue<Vector2>();
        rotx -= lookInput.y * Game.get.settings.aimSense * Time.deltaTime;
        rotx = Mathf.Clamp(rotx,-maxLookY,maxLookY);
        roty += lookInput.x * Game.get.settings.aimSense * Time.deltaTime;
        camera.transform.localEulerAngles = new Vector3(rotx,0,0);
        transform.localEulerAngles = new Vector3(0,roty,0);
    }
    
    //built-in tapping checks for some reason does not work so I had to implement my own
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

    void ApplyPhysics()
    {
        // When player hits the ground
        if (onGround && velocity.y < 0)
        {
            velocity = Vector3.zero;
        }

        //Gravity
        if(gravityOn) velocity.y -= gravityStrength * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void MovementControls()
    {
        float xMoveInput = Game.input.Player.Move.ReadValue<Vector2>().x;
        if(lockOnTarget)
        {
            float normalizeDistance = Util.Remap(Vector3.Distance(transform.position, lockOnTarget.position),0,lockOnDistance, 0,1);
            float invertedDistance = Util.InvertRange(normalizeDistance,0,1);
            xMoveInput *= invertedDistance;
        }
        float zMoveInput = Game.input.Player.Move.ReadValue<Vector2>().y;
        float magnitude = Game.input.Player.Move.ReadValue<Vector2>().magnitude;
        Vector3 moveDirection = (transform.right * xMoveInput + transform.forward * zMoveInput).normalized * magnitude;

        
        if(DashKeyboardInput() && !dashing)
        {
            if(lockOnTarget)
            {
                StartCoroutine(DirectionalDash(prevDashInput, true));
            }
            else
            {
                StartCoroutine(DirectionalDash(prevDashInput, false));
            }
        }
        else if(Gamepad.current.leftShoulder.isPressed && !dashing)
        {
            if(lockOnTarget)
            {
                StartCoroutine(DirectionalDash(new Vector2(xMoveInput,zMoveInput), true));
            }
            else
            {
                StartCoroutine(DirectionalDash(prevDashInput, false));
            }
        }

        if(!dashing)
        {
            controller.Move(moveDirection * currentMoveSpeed * Time.deltaTime);
        }
        
        //fixes falling down slopes issue
        if(onGround && moveDirection.magnitude > 0 && !jumping)
        {
            Physics.Raycast(transform.position,Vector3.down,out RaycastHit hit,groundDistance);
            controller.Move(Vector3.down * hit.distance);
        }

        prevDashInput = new Vector2(xMoveInput,zMoveInput);

        if(Game.input.Player.Jump.WasPerformedThisFrame() && onGround)
        {
            StartCoroutine(Jump());
        }
    }

    void CombatControls()
    {
        actionVector = Game.input.Player.Look.ReadValue<Vector2>();

        if(Game.input.Player.Attack.WasPerformedThisFrame())
        {
            if(Game.get.settings.autoLockOn) LockOn();
            atkAngle = Mathf.Atan2(actionVector.x, -actionVector.y) * 180 / Mathf.PI;
            animator.SetTrigger("cut");
        }
        
        if(Game.input.Player.Defend.IsPressed())
        {
            
        }
        else
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

    IEnumerator DirectionalDash(Vector2 dashInput,bool lockedOn)
    {
        dashing = true;
        float t = 0;
        Vector3 dashDirection = (transform.right * dashInput.x + transform.forward * dashInput.y).normalized;
        if(lockedOn)
        {
            float normalizeDistance = Util.Remap(Vector3.Distance(transform.position, lockOnTarget.position),0,lockOnDistance, 0,1);
            float invertedDistance = Util.InvertRange(normalizeDistance,0,1);
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
    
    public void LockOn()
    {
        Ray ray = new Ray(camera.position, camera.forward);
        RaycastHit[] hits =  Physics.RaycastAll(ray, lockOnDistance, lockOnLayerMask);
        if(hits.Length == 0) return;
        Transform closest = hits[0].transform;
        
        foreach(RaycastHit hit in hits)
        {
            //If current iteration > closest
            float currentDot  = Vector3.Dot((hit.transform.position - camera.position).normalized, camera.forward);
            float closestDot = Vector3.Dot((closest.position - camera.position).normalized,camera.forward);
            if (currentDot > closestDot)
            {
                closest = hit.transform;
            }
        }
        
        lockOnTarget = closest;
    }
    public void LockOff()
    {
        lockOnTarget = null;
        lockOnLerp = 0;
    }
    public void AttackState()
    {
        armPivot.localEulerAngles = new Vector3(0,0,atkAngle);
        state = State.ATTACKING;
    }
    public void DefendState()
    {
        armPivot.localEulerAngles = Vector3.zero;
        state = State.DEFENDING;
    }
    public void IdleState()
    {
        if(Game.get.settings.autoLockOn) LockOff();
        armPivot.localEulerAngles = Vector3.zero;
        state = State.IDLE;
    }

}
