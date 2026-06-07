using UnityEngine;
//using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //Components
    [Header("Components")]
    public Camera camera;
    public CharacterController controller;
    public Animator animator;
    public Transform armPivot;
    public Weapon weapon;
    public GameObject magicSlashProjectile;
    
    //States
    PlayerState currentState;
    public PlayerIDLE idle = new PlayerIDLE();
    public PlayerAtk atk = new PlayerAtk();
    public PlayerDEF def = new PlayerDEF();
    public PlayerHIT hit = new PlayerHIT();
    public PlayerDEAD dead = new PlayerDEAD();

    //For Basic Controls
    [Header("General")]
    [HideInInspector] public float lookSpeed;
    [HideInInspector] public float moveSpeed = 20;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [HideInInspector] public float atkAngle = 0;

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

    //For Evasion
    public float evasionSpeed = 200;
    Vector3 evadeDirection;
    float evadeTimer;
    bool evading;


    //Events
    void Start()
    {
        currentState = idle;
        currentState.Enter(this);
        lookSpeed = Game.aimSense;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        currentState.Update(this);
        ApplyForces();
    }

    void FixedUpdate()
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        grounded = Physics.Raycast(groundRay, out slopeHit, groundDistance, groundLayer);
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

        if(Game.controls.Player.Evade.WasPressedThisFrame() && grounded)
        {
            evadeTimer = 0.1f;
            float x = Game.controls.Player.Move.ReadValue<Vector2>().normalized.x;
            float z = Game.controls.Player.Move.ReadValue<Vector2>().normalized.y;
            evadeDirection = (transform.right * x + transform.forward * z).normalized;
            evading = true;
        }

        if(evading)
        {
            if(evadeTimer > 0)
            {
                controller.Move(evadeDirection * evasionSpeed * Time.deltaTime);
                evadeTimer -= Time.deltaTime;
            }
            else
            {
                evading = false;
            }
        }
        else
        {
            // Moving Around 
            float x = Game.controls.Player.Move.ReadValue<Vector2>().x;
            float z = Game.controls.Player.Move.ReadValue<Vector2>().y;
            float m = Game.controls.Player.Move.ReadValue<Vector2>().magnitude;
            Vector3 moveDirection = (transform.right * x + transform.forward * z).normalized * m;
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

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

    public void Look()
    {
        // Mouse Look
        float lx = Game.controls.Player.Look.ReadValue<Vector2>().x;
        float ly = Game.controls.Player.Look.ReadValue<Vector2>().y;

        // Looking up/down with camera
        xRot -= ly * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -45, 45);
        camera.transform.localRotation = Quaternion.Euler(xRot, 0, 0);

        // Looking left right with player body
        yRot += lx * lookSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, yRot, 0);
    }

    public void SwitchState(PlayerState state)
    {
        currentState = state;
        currentState.Enter(this);

        if(currentState == idle)
        {
            Debug.Log("Back To Idle");
        }
        else if(currentState == atk)
        {
            Debug.Log("Attack!!!");
        }
        else if(currentState == def)
        {
            Debug.Log("On Gaurd");
        }
    }

    void ApplyForces()
    {
        //Gravity
        velocity.y += -10 * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    //Animation Events
    public void StartAtk()
    {
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        StartCoroutine(weapon.AnimateTrail());
    }
    public void EndAtk()
    {
        if (weapon.isMagical)
        {
            var slash = Instantiate(magicSlashProjectile);
            Projectile p = slash.GetComponent<Projectile>();
            slash.transform.position = camera.transform.position + camera.transform.forward;
            Vector3 baseRot = Quaternion.LookRotation(camera.transform.forward).eulerAngles;
            slash.transform.localEulerAngles = baseRot + new Vector3(0, 0, atkAngle - 90);

            p.owner = gameObject;
            p.damage = weapon.damage * 2;
            p.direction = camera.transform.forward;
        }
        SwitchState(idle);
        lookSpeed = Game.aimSense;
        armPivot.localEulerAngles = Vector3.zero;
    }

}
