using System.Collections;
using System.Collections.Generic;
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
    public List<PlayerAbility> perks = new List<PlayerAbility>();


    [Header("Movement")]
    [Min(0)] public int maxNumberOfJumps = 1;
    [Min(1)] public float jumpHeight = 1;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [Min(2)] public float normalSpeed = 20;
    [Min(1)] public float crouchSpeed = 10;
    [HideInInspector] public float targetSpeed = 0;
    [HideInInspector] public float currentSpeed = 0;
    bool crouching = false;
    bool jumping = false;
    int jumpsTaken = 0;


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
    bool rotateReticle = true;
    [HideInInspector] public bool isAttacking = false;


    //Events
    void Start()
    {
        lookSpeed = Game.settings.aimSense;
        Cursor.lockState = CursorLockMode.Locked;

        foreach(PlayerAbility perk in perks)
        {
            perk.Init();
        }
    }

    void Update()
    {
        
        foreach(PlayerAbility perk in perks)
        {
            perk.Update();
        }

        Combat();
        FreeLook();
        ApplyPhysics();
        Movement();
    }

    void FixedUpdate()
    {
        
        foreach(PlayerAbility perk in perks)
        {
            perk.FixedUpdate();
        }

        currentSpeed = Mathf.Lerp(currentSpeed,targetSpeed, 10 * Time.deltaTime);
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
        if(rotateReticle) reticle.rectTransform.rotation = Quaternion.Euler(0,0,atkAngle);
        
        if(Game.input.Player.Attack.WasPerformedThisFrame())
        {   
            animator.SetTrigger("atk");
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

    void Movement()
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
            if(crouching)
            {
                cameraHolder.transform.localPosition = new Vector3(0,1,0);
                controller.center = Vector3.up * 0.5f;
                controller.height = 1;
                crouching = false;
            }
            else
            {
                cameraHolder.transform.localPosition = new Vector3(0,2,0);
                controller.center = Vector3.up;
                controller.height = 2;
                crouching = true;
            }
        }
    }
    
    IEnumerator Jump()
    {
        jumping = true;
        velocity.y = Mathf.Sqrt(jumpHeight * 2 * gravityStrength);
        jumpsTaken++;
        yield return new WaitForSeconds(0.25f);
        jumping = false;
    }

    //Animation events
    public void StartAttack()
    {
        armPivot.localEulerAngles = new Vector3(0,0,atkAngle);
        isAttacking = true;
        rotateReticle = false;
    }
    public void EndAttack()
    {
        armPivot.localEulerAngles = Vector3.zero;
        isAttacking = false;
        rotateReticle = true;
    }
}
