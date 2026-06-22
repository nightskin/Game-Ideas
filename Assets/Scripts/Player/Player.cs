using UnityEngine;

public class Player : MonoBehaviour
{
    //Components
    [Header("Components")]
    public Transform camera;
    public CharacterController controller;
    public Animator animator;
    
    //States
    PlayerState currentState;
    public PlayerALIVE alive = new PlayerALIVE();
    public PlayerHIT hit = new PlayerHIT();
    public PlayerDEAD dead = new PlayerDEAD();

    //For Movement
    [Header("Movement")]
    [HideInInspector] public float lookSpeed;
    [HideInInspector] public float moveSpeed = 20;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [HideInInspector] RaycastHit slopeHit;
    
    [Header("Looking")]
    [Range(0,90)] public float maxRotX = 45;
    [Range(-90,0)] public float minRotX = -45;
    public float cameraBobSpeed = 5;
    public float cameraBobHeight = 0.25f;
    float rx = 0;
    float ry = 0;

    // For Jumping Around
    [Header("Jumping")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField][Min(0)] float groundDistance = 0.5f;
    [HideInInspector] public bool grounded;
    [Min(1)] public float jumpHeight = 3;
    [HideInInspector] public bool jumping = false;
    

    //Events
    void Start()
    {
        currentState = alive;
        currentState.Enter(this);
        lookSpeed = Game.aimSense;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        currentState.Update(this);
    }

    void FixedUpdate()
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        grounded = Physics.Raycast(groundRay, out slopeHit, groundDistance, groundLayer);


    }

    //Functions
    public void Look()
    {
        Vector2 lookInput = Game.controls.Player.Look.ReadValue<Vector2>();
        rx -= lookInput.y * Game.aimSense * Time.deltaTime;
        rx = Mathf.Clamp(rx,minRotX,maxRotX);
        ry += lookInput.x * Game.aimSense * Time.deltaTime;
        camera.transform.localEulerAngles = new Vector3(rx,0,0);
        transform.localEulerAngles = new Vector3(0,ry,0);
    }

    public void Move()
    {
        // When player hits the ground
        if (grounded && velocity.y < 0)
        {
            velocity.y = 0;
            if(jumping) jumping = false;
        }

        // Moving Around 
        float x = Game.controls.Player.Move.ReadValue<Vector2>().x;
        float z = Game.controls.Player.Move.ReadValue<Vector2>().y;
        float m = Game.controls.Player.Move.ReadValue<Vector2>().magnitude;
        Vector3 moveDirection = (transform.right * x + transform.forward * z).normalized * m;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        
        //Gravity
        velocity.y += -10 * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //Jumping
        if(Game.controls.Player.Jump.IsPressed() && grounded)
        {
            velocity = Vector3.up * Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
            jumping = true;
        }

        //Handle Moving Down slopes
        if (grounded && !jumping)
        {
            controller.Move(new Vector3(0, -slopeHit.distance, 0));
        }
    }
    
    public void SwitchState(PlayerState state)
    {
        currentState = state;
        currentState.Enter(this);

        if(currentState == alive)
        {
            Debug.Log("LIVE");
        }
        else if(currentState == hit)
        {
            Debug.Log("OW!!!");
        }
        else if(currentState == dead)
        {
            Debug.Log("You Dead");
        }
    }


    //Animation Events
    

}
