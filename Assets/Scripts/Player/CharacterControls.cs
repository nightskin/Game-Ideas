using System.Collections;
using UnityEngine;

public class CharacterControls : MonoBehaviour
{
    [Header("Components")]
    public Transform camera;
    public CharacterController controller;
    public Animator animator;
    

    [Header("Movement")]
    public Vector3 velocity = Vector3.zero;
    public float speed = 20;
    Vector3 moveDirection;
    float currentMoveSpeed;

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

    [Header("Physics")]
    public bool gravityOn = true; 
    public float gravityStrength = 10;
    [SerializeField][Min(0)] float groundDistance = 0.5f;
    [HideInInspector] public bool grounded;

    [Header("Dashing")]
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
        Vector2 lookInput = Game.controls.Player.Look.ReadValue<Vector2>();
        Vector3 lockOnOffset = ((lockOnTarget.transform.up * lookInput.y) + (camera.transform.right * lookInput.x)).normalized;

        //Looking
        Vector3 lookDirection = lockOnTarget.transform.position - camera.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);

        rotx = Mathf.LerpAngle(rotx, lookRotation.eulerAngles.x, 10 * Time.deltaTime);
        roty = Mathf.LerpAngle(roty, lookRotation.eulerAngles.y, 10 * Time.deltaTime);

        camera.transform.localEulerAngles = new Vector3(rotx,roty,0);

        if(Vector3.Distance(transform.position, lockOnTarget.position) > lockOnDistance)
        {
            lockOnTarget = null;
        }
    }
    
    void FreeLook()
    {
        //Looking Around
        Vector2 lookInput = Game.controls.Player.Look.ReadValue<Vector2>();
        rotx -= lookInput.y * Game.aimSense * Time.deltaTime;
        rotx = Mathf.Clamp(rotx,-maxLookY,maxLookY);
        roty += lookInput.x * Game.aimSense * Time.deltaTime;
        camera.transform.localEulerAngles = new Vector3(rotx,roty,0);
    }

    void Movement()
    {
        // When player hits the ground
        if (grounded && velocity.y < 0)
        {
            velocity = Vector3.zero;
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
        moveDirection = (camera.transform.right * x + new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z) * z).normalized * m;



        if(!dashing) controller.Move(moveDirection * currentMoveSpeed * Time.deltaTime);
        
        //Fixes Moving Down Slopes
        if(grounded && moveDirection.magnitude > 0)
        {
            Physics.Raycast(transform.position,Vector3.down,out RaycastHit hit,groundDistance);
            controller.Move(Vector3.down * hit.distance);
        }

        //Gravity
        if(gravityOn) velocity.y -= gravityStrength * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //Dashing
        if(Game.controls.Player.Dash.WasPerformedThisFrame() && !dashing)
        {
            StartCoroutine(Dash(0.1f, 200));
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

    IEnumerator Dash(float time, float speed)
    {
        float t = 0;
        dashing = true;
        while(t < time)
        {
            controller.Move(moveDirection * speed * Time.deltaTime);
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
