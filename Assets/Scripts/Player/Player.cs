using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //Components
    [Header("Components")]
    public PlayerHUD hud;
    public Camera camera;
    public CharacterController controller;
    public Animator animator;
    public Transform armPivot;
    public Weapon weapon;
    [SerializeField] GameObject slashProjectile;
    [SerializeField] GameObject blastProjectile;

    //For Basic Controls
    [Header("General")]
    [HideInInspector] public float lookSpeed;
    [SerializeField][Min(1)] float moveSpeed = 25;
    [HideInInspector] public Vector3 velocity = Vector3.zero;

    RaycastHit slopeHit;
    float xRot = 0;
    float yRot = 0;
    Vector3 moveDirection;


    //For Combat
    public enum PlayerState
    {
        IDLE,
        ATK,
        DEF,
    }
    [HideInInspector] public PlayerState state = PlayerState.IDLE;
    Vector2 actionVector = Vector2.zero;
    Vector2 defVector = Vector2.zero;
    float atkAngle;


    //Stun Variables For When The Player is Hit
    [HideInInspector] public bool wasHit = false;
    [HideInInspector] public Vector3 knockBackForce;
    [Range(0, 1)] float stunTime = 0.05f;
    float stunTimer = 0;

    // For Jumping Around
    [Header("Jumping Variables")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField][Min(0)] float groundDistance = 0.5f;
    [HideInInspector] public bool grounded;
    [SerializeField] bool jumpingEnabled;
    [SerializeField][Min(1)] float jumpHeight = 3;
    int numberOfJumps = 0;
    bool jumping = false;

    //For Dashing
    [Header("Dashing")]
    bool dashing = false;
    [SerializeField] float dashSpeed = 150;
    float dashTime = 0.1f;
    float dashTimer = 0;
    Vector3 dashDirection;


    void Start()
    {
        lookSpeed = Game.aimSense;
        Cursor.lockState = CursorLockMode.Locked;
        stunTimer = stunTime;


        Game.controls.Player.Jump.performed += Jump_performed;
        Game.controls.Player.Dash.performed += Dash_performed;
        Game.controls.Player.Slash.canceled += Slash_canceled;
        Game.controls.Player.Slash.performed += Slash_performed;

    }

    void Update()
    {
        Movement();
        Combat();
    }

    void FixedUpdate()
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        grounded = Physics.Raycast(groundRay, out slopeHit, groundDistance, groundLayer);
    }

    void OnDestroy()
    {   
        Game.controls.Player.Jump.performed -= Jump_performed;
        Game.controls.Player.Dash.performed -= Dash_performed;
        Game.controls.Player.Slash.canceled -= Slash_canceled;
    }
    
    private void Jump_performed(InputAction.CallbackContext obj)
    {
        // Normal Jump
        if (jumpingEnabled)
        {
            if (grounded)
            {
                velocity = Vector3.up * Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
                jumping = true;
            }
        }
    }

    private void Dash_performed(InputAction.CallbackContext obj)
    {
        dashing = true;
    }

    private void Slash_performed(InputAction.CallbackContext obj)
    {
        hud.reticle.SetActive(true);
    }

    private void Slash_canceled(InputAction.CallbackContext obj)
    {
        lookSpeed *= Game.slowCameraAtkAmount;
        animator.SetTrigger("slash");
        hud.reticle.SetActive(false);
    }


    void Combat()
    {
        if (wasHit)
        {

        }
        else
        {
            if (Game.controls.Player.Slash.IsPressed())
            {
                actionVector = Game.controls.Player.Look.ReadValue<Vector2>();
                atkAngle = Mathf.Atan2(actionVector.x, -actionVector.y) * 180 / Mathf.PI;
                hud.reticle.transform.rotation = Quaternion.Euler(0,0,atkAngle);
            }
        }
    }
    void Movement()
    {
        // Mouse Look
        float lx = Game.controls.Player.Look.ReadValue<Vector2>().x;
        float ly = Game.controls.Player.Look.ReadValue<Vector2>().y;

        //Looking up/down with camera
        xRot -= ly * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -45, 45);
        camera.transform.localRotation = Quaternion.Euler(xRot, 0, 0);

        //Looking left right with player body
        yRot += lx * lookSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, yRot, 0);

        // Moving Around
        if (grounded && velocity.y < 0)
        {
            numberOfJumps = 0;
            velocity = Vector3.zero;
            if(jumping) jumping = false;
        }

        float x = Game.controls.Player.Move.ReadValue<Vector2>().x;
        float z = Game.controls.Player.Move.ReadValue<Vector2>().y;
        float m = Game.controls.Player.Move.ReadValue<Vector2>().magnitude;
        moveDirection = (transform.right * x + transform.forward * z).normalized * m;

        if(m > 0)
        {
            animator.SetBool("moving", true);
        }
        else
        {
            animator.SetBool("moving", false);
        }

        if (dashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer > 0)
            {
                controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            }
            else
            {
                dashing = false;
            }
        }
        else
        {
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        //Gravity
        velocity += new Vector3(0, -10, 0) * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //Handle Moving Down slopes
        if (grounded && !jumping)
        {
            controller.Move(new Vector3(0, -slopeHit.distance, 0));
        }
    }

    //Animation Events
    public void StartAtk()
    {
        state = PlayerState.ATK;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        StartCoroutine(weapon.AnimateTrail());
        defVector = Vector2.zero;
        animator.SetFloat("x", defVector.x);
        animator.SetFloat("y", defVector.y);
    }
    public void EndAtk()
    {
        state = PlayerState.IDLE;
        lookSpeed = Game.aimSense;
        armPivot.localEulerAngles = Vector3.zero;
    }
    public void MagicBlast()
    {
        if(weapon.isMagical)
        {
            var blast = Instantiate(blastProjectile);
            Projectile p = blast.GetComponent<Projectile>();

            p.owner = gameObject;
            p.damage = weapon.damage;
            p.direction = camera.transform.forward;
        }
    }

    public void MagicSwipe()
    {
        if (weapon.isMagical)
        {
            var slash = Instantiate(slashProjectile);
            Projectile p = slash.GetComponent<Projectile>();
            slash.transform.position = camera.transform.position + camera.transform.forward;
            Vector3 baseRot = Quaternion.LookRotation(camera.transform.forward).eulerAngles;
            slash.transform.localEulerAngles = baseRot + new Vector3(0, 0, atkAngle - 90);

            p.owner = gameObject;
            p.damage = weapon.damage * 2;
            p.direction = camera.transform.forward;
        }
    }
    public void StartBlock()
    {
        state = PlayerState.DEF;
        armPivot.localEulerAngles = Vector3.zero;
    }
    public void BackToIdle()
    {
        state = PlayerState.IDLE;
        defVector = Vector2.zero;
        animator.SetFloat("x", defVector.x);
        animator.SetFloat("y", defVector.y);
    }

}
