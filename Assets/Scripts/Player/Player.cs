using UnityEngine;

public class Player : MonoBehaviour
{
    //Components
    [Header("Components")]
    public PlayerCamera camera;
    public CharacterController controller;
    public Animator animator;
    
    //States
    PlayerState currentState;
    public PlayerALIVE alive = new PlayerALIVE();
    public PlayerHIT hit = new PlayerHIT();
    public PlayerDEAD dead = new PlayerDEAD();

    //For Basic Controls
    [Header("General")]
    [HideInInspector] public float lookSpeed;
    [HideInInspector] public float moveSpeed = 20;
    [HideInInspector] public Vector3 velocity = Vector3.zero;

    [HideInInspector] public RaycastHit slopeHit;
    float xRot = 0;
    float yRot = 0;

    // For Jumping Around
    [Header("Jumping Variables")]
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

        //Gravity
        velocity.y += -10 * Time.fixedDeltaTime;
        controller.Move(velocity * Time.fixedDeltaTime);
    }

    //Functions
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
