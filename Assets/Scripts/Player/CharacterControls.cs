using System.Collections;
using System.Data;
using UnityEngine;

public class CharacterControls : MonoBehaviour
{
    //Components
    [Header("Components")]
    public Transform camera;
    public CharacterController controller;
    public Animator animator;
    public Ability_WallRun WallRunAbility;
    

    //For Movement
    [Header("Movement")]
    public Vector3 velocity = Vector3.zero;
    [SerializeField] float moveSpeed = 20;
    Vector3 moveDirection;
    [HideInInspector] RaycastHit slopeHit;
    
    [Header("Looking")]
    [Range(0,90)] public float maxLookY = 45;
    [SerializeField] float cameraBobSpeed = 5;
    [SerializeField] float cameraBobHeight = 0.25f;
    float rx = 0;
    float ry = 0;
    Vector2 lookDirection;

    // For Jumping Around
    [Header("Jumping")]
    public bool gravityOn = true; 
    public float gravityStrength = 10;
    [SerializeField][Min(0)] float groundDistance = 0.5f;
    [HideInInspector] public bool grounded;
    [SerializeField] int maxJumps = 2;
    [SerializeField] float jumpHeight = 3;
    bool decelerating = false;
    [HideInInspector] public int numJumps = 0;


    //Events
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if(!WallRunAbility) WallRunAbility = GetComponent<Ability_WallRun>();
    }

    void Update()
    {
        LookAround();
        Movement();
    }

    void FixedUpdate()
    {
        // Checks If player is grounded
        Ray groundRay = new Ray(transform.position, Vector3.down);
        grounded = Physics.Raycast(groundRay, out slopeHit, groundDistance);
    }

    //Functions
    void LookAround()
    {
        lookDirection = Game.controls.Player.Look.ReadValue<Vector2>();
        rx -= lookDirection.y * Game.aimSense * Time.deltaTime;
        rx = Mathf.Clamp(rx,-maxLookY,maxLookY);
        ry += lookDirection.x * Game.aimSense * Time.deltaTime;
        camera.transform.localEulerAngles = new Vector3(rx,0,0);
        transform.localEulerAngles = new Vector3(0,ry,0);
    }

    void Movement()
    {
        // When player hits the ground
        if (grounded && velocity.y < 0)
        {
            velocity.y = 0;
            numJumps = 0;
        }

        // Moving Around 
        float x = Game.controls.Player.Move.ReadValue<Vector2>().x;
        float z = Game.controls.Player.Move.ReadValue<Vector2>().y;
        float m = Game.controls.Player.Move.ReadValue<Vector2>().magnitude;
        moveDirection = (transform.right * x + transform.forward * z).normalized * m;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        
        //Fixes Moving Down Slopes
        if(grounded && moveDirection.magnitude > 0)
        {
            Physics.Raycast(transform.position,Vector3.down,out RaycastHit hit,groundDistance);
            controller.Move(Vector3.down * hit.distance);
        }
        
        //Gravity
        if(gravityOn) velocity.y -= gravityStrength * Time.deltaTime;

        if((velocity.x != 0 || velocity.z != 0) && !decelerating)
        {
            StartCoroutine(ApplyDrag(0.1f));
        }

        controller.Move(velocity * Time.deltaTime);

        //Jumping
        if(Game.controls.Player.Jump.WasPressedThisFrame() && numJumps < maxJumps)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2 * gravityStrength);
            numJumps++;
        }

    }
    
    IEnumerator ApplyDrag(float amount)
    {
        float t = 0;
        decelerating = true;
        while(t < 1)
        {
            t += amount * Time.deltaTime;
            velocity.x = Mathf.Lerp(velocity.x, 0, t);
            velocity.z = Mathf.Lerp(velocity.z, 0, t);
            yield return null;
        }
        decelerating = false;
    }

    

    //Animation Events
    

}
