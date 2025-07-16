using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //Components
    [Header("Components")]
    Controls controls;
    public static Controls.PlayerActions actions;
    [SerializeField] Camera camera;
    [SerializeField] CharacterController controller;
    public Animator animator;
    [SerializeField] Transform armPivot;
    [SerializeField] Transform arm;
    [SerializeField] PlayerWeapon weapon;


    //For Basic Controls
    [Header("General")]
    [SerializeField][Min(1)] float moveSpeed = 10;
    [SerializeField] float cameraBobSpeed = 2.0f;
    [SerializeField] float armSwaySpeed = 0.1f;


    Vector3 moveDirection;
    float lookSpeed;
    float xRot = 0;
    float yRot = 0;


    //For Jumping and falling
    [Header("Jumping")]
    [SerializeField] float jumpHeight = 3;
    [SerializeField] LayerMask jumpLayer;
    [SerializeField] float gravity = 10.0f;
    bool grounded = false;
    Vector3 velocity = Vector3.zero;

    //LockOn System
    [Header("LockOnSystem")]
    [SerializeField] float lockOnSpeed = 20;
    [SerializeField] float lockOnDistance = 500;
    [SerializeField] LayerMask lockOnLayer;
    [HideInInspector] public Transform target = null;


    //For Combat Systems
    [Header("CombatControls")]
    [SerializeField][Min(1)] float normalLookSpeed = 100;
    [SerializeField][Min(0)] float combatLookSpeed = 10;
    Vector2 actionVector = Vector2.zero;
    float atkAngle = 180;
    [HideInInspector] public bool stunned = false;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        camera = Camera.main;
        lookSpeed = normalLookSpeed;

        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();

        actions.Jump.performed += Jump_performed;
        actions.LockOn.performed += LockOn_performed;
        actions.Attack.performed += Attack_performed;
        actions.Defend.performed += Defend_performed;
        actions.Defend.canceled += Defend_canceled;
    }

    void Update()
    {
        if (target != null)
        {
            LookAtTarget();
        }
        else
        {
            LookAround();
        }

        Movement();
        CombatControls();


    }

    void FixedUpdate()
    {
        grounded = Physics.CheckSphere(transform.position, controller.radius, jumpLayer);
    }

    void OnDestroy()
    {
        actions.Jump.performed -= Jump_performed;
        actions.LockOn.performed -= LockOn_performed;
        actions.Attack.performed -= Attack_performed;
    }

    private void Jump_performed(InputAction.CallbackContext obj)
    {
        if (grounded)
        {
            velocity = Vector3.up * Mathf.Sqrt(jumpHeight * 2 * gravity);
        }
    }

    private void LockOn_performed(InputAction.CallbackContext context)
    {
        if (target == null)
        {
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, lockOnDistance, lockOnLayer))
            {
                target = hit.transform;
            }
        }
        else
        {
            target = null;
        }
    }

    private void Attack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //lookSpeed = combatLookSpeed;
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, lockOnDistance, lockOnLayer))
        {
            target = hit.transform;
        }
        animator.SetTrigger("slash");
    }

    private void Defend_performed(InputAction.CallbackContext context)
    {
        lookSpeed = combatLookSpeed;
    }

    private void Defend_canceled(InputAction.CallbackContext context)
    {
        lookSpeed = normalLookSpeed;
    }

    void Movement()
    {
        if (grounded && velocity.y < 0)
        {
            velocity = Vector3.zero;
        }

        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = (transform.right * x + transform.forward * z).normalized;
        controller.Move(moveDirection * moveSpeed * actions.Move.ReadValue<Vector2>().magnitude * Time.deltaTime);

        //Camera Bob
        if (moveDirection.magnitude > 0 && grounded)
        {
            camera.transform.localPosition = Vector3.up * (2 + Mathf.PingPong(Time.time * cameraBobSpeed, 1));
            arm.transform.localPosition = new Vector3(0, Mathf.PingPong(Time.time * armSwaySpeed, 0.1f), 0);
        }


        //Gravity
        velocity += Vector3.down * gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void LookAround()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;

        //Looking up/down with camera
        xRot -= y * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -45, 45);
        camera.transform.localEulerAngles = new Vector3(xRot, 0, 0);

        //Looking left right with player body
        yRot += x * lookSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);

    }

    void LookAtTarget()
    {
        Vector3 dirToTarget = (target.position - camera.transform.position).normalized;
        Vector3 rotToTarget = Quaternion.LookRotation(dirToTarget).eulerAngles;

        xRot = Mathf.LerpAngle(xRot, rotToTarget.x, lockOnSpeed * Time.deltaTime);
        camera.transform.localEulerAngles = new Vector3(xRot, 0, 0);

        yRot = Mathf.LerpAngle(yRot, rotToTarget.y, lockOnSpeed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(0, yRot, 0);


    }

    void CombatControls()
    {
        actionVector = actions.Look.ReadValue<Vector2>().normalized;
        if (actions.Attack.IsPressed())
        {
            if (actionVector.magnitude > 0.1f)
            {
                atkAngle = Mathf.Atan2(actionVector.x, -actionVector.y) * 180 / Mathf.PI;
            }
        }
        else if (actions.Defend.IsPressed())
        {
            if (actionVector.magnitude > 0.1f)
            {
                animator.SetInteger("defX", Mathf.RoundToInt(actionVector.x));
                animator.SetInteger("defY", Mathf.RoundToInt(actionVector.y));
            }
        }
    }

    //Animation Events
    public void StartAttack()
    {
        weapon.state = PlayerWeapon.WeaponState.ATTACKING;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
    }

    public void EndAttack()
    {
        weapon.state = PlayerWeapon.WeaponState.IDLE;
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        lookSpeed = normalLookSpeed;
        animator.SetInteger("defX", 0);
        animator.SetInteger("defY", 0);
    }

    public void StartDefense()
    {
        weapon.state = PlayerWeapon.WeaponState.DEFENDING;
    }

    public void EndDefense()
    {
        weapon.state = PlayerWeapon.WeaponState.IDLE;
    }

    public void Recoil()
    {
        weapon.state = PlayerWeapon.WeaponState.IDLE;
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        animator.SetInteger("defX", 0);
        animator.SetInteger("defY", 0);
    }
}
