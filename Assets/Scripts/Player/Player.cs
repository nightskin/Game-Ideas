using System.Runtime.Serialization.Formatters;
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
    float moveSpeed;
    public static float lookSpeed;

    [SerializeField] float cameraBobSpeed = 2.0f;
    [SerializeField] float armSwaySpeed = 0.1f;
    [SerializeField] LayerMask groundLayer;

    Vector3 moveDirection;
    float xRot = 0;
    float yRot = 0;

    bool dashing = false;

    [SerializeField][Min(1)] float walkSpeed = 25;
    [SerializeField] float dashSpeed = 150;
    [SerializeField][Min(2)] float runSpeed = 50;

    float dashTime = 0.1f;
    float dashTimer = 0;

    //LockOn System
    [Header("LockOnSystem")]
    [SerializeField] float lockOnDistance = 500;
    [SerializeField] LayerMask lockOnLayer;
    [HideInInspector] public Transform lockOnTarget = null;
    bool lockedOn = false;


    //For Combat Systems
    [Header("CombatControls")]
    [SerializeField][Range(0,1)] float atkDamp = 0.1f;
    [SerializeField][Range(0,1)] float defDamp = 0.1f;
    Vector2 actionVector = Vector2.zero;
    float atkAngle = 0;
    [HideInInspector] public bool stunned = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        camera = Camera.main;
        moveSpeed = walkSpeed;
        lookSpeed = GameSettings.aimSensitivity;

        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();

        actions.Sprint.canceled += Sprint_canceled;
        actions.Dash.performed += Dash_performed;
        actions.LockOn.performed += LockOn_performed;
        actions.Attack.performed += Attack_performed;
        actions.Defend.performed += Defend_performed;
        actions.Defend.canceled += Defend_canceled;
    }

    void Update()
    {
        if (lockOnTarget != null)
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
        //Make Sure Player Is Always on ground
        if (moveDirection.magnitude > 0)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,1,groundLayer))
            {
                controller.transform.position = hit.point;
            }
        }

    }

    void OnDestroy()
    {
        actions.Sprint.canceled -= Sprint_canceled;
        actions.Dash.performed -= Dash_performed;
        actions.LockOn.performed -= LockOn_performed;
        actions.Attack.performed -= Attack_performed;
        actions.Defend.performed -= Defend_performed;
        actions.Defend.canceled -= Defend_canceled;
    }

    private void Dash_performed(InputAction.CallbackContext obj)
    {
        if (!dashing)
        {
            dashing = true;
            moveSpeed = dashSpeed;
            dashTimer = dashTime;
        }
    }

    private void Sprint_canceled(InputAction.CallbackContext obj)
    {
        if (!dashing)
        {
            moveSpeed = walkSpeed;
        }

    }

    private void LockOn_performed(InputAction.CallbackContext context)
    {
        if (lockOnTarget == null)
        {
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, lockOnDistance, lockOnLayer))
            {
                lockOnTarget = hit.transform;
                lockedOn = true;
            }
        }
        else
        {
            lockOnTarget = null;
            lockedOn = false;
        }
    }

    private void Attack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!lockedOn && lockOnTarget != null)
        {
            Ray ray = new Ray(camera.transform.position, camera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, lockOnDistance, lockOnLayer))
            {
                lockOnTarget = hit.transform;
            }            
        }

        if(GameSettings.slowCameraMovementWhenAttacking) lookSpeed *= atkDamp;
        animator.SetTrigger("slash");

    }

    private void Defend_performed(InputAction.CallbackContext context)
    {
        if(GameSettings.slowCameraMovementWhenDefending) lookSpeed *= defDamp;
        weapon.SetState(PlayerWeapon.WeaponState.DEFENDING);
    }

    private void Defend_canceled(InputAction.CallbackContext context)
    {
        if (GameSettings.slowCameraMovementWhenDefending) lookSpeed = GameSettings.aimSensitivity;
        animator.SetFloat("x", 0);
        animator.SetFloat("y", 0);
        weapon.SetState(PlayerWeapon.WeaponState.IDLE);
    }

    void Movement()
    {
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = (transform.right * x + transform.forward * z).normalized;
        if (dashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer > 0)
            {
                controller.Move(moveDirection * moveSpeed * Time.deltaTime);
            }
            else
            {
                moveSpeed = walkSpeed;
                dashing = false;
            }
        }
        else
        {
            if (actions.Sprint.IsPressed() && z > 0.5f)
            {
                moveSpeed = runSpeed;
            }
            else
            {
                moveSpeed = walkSpeed;
            }
            controller.Move(moveDirection * moveSpeed * actions.Move.ReadValue<Vector2>().magnitude * Time.deltaTime);
        }
        
        //Camera Bob
        if (GameSettings.cameraBob)
        {
            if (moveDirection.magnitude > 0 && !dashing)
            {
                camera.transform.localPosition = Vector3.up * (1.75f + Mathf.PingPong(Time.time * cameraBobSpeed, 1));
                arm.transform.localPosition = new Vector3(0, Mathf.PingPong(Time.time * armSwaySpeed, 0.1f), 0);
            }
        }
    }

    void LookAround()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;

        //Looking up/down with camera
        xRot -= y * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -45, 45);
        camera.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(camera.transform.localEulerAngles.x, xRot, 20 * Time.deltaTime), 0, 0);

        //Looking left right with player body
        yRot += x * lookSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, Mathf.LerpAngle(transform.localEulerAngles.y, yRot, 20 * Time.deltaTime), 0);

    }

    void LookAtTarget()
    {
        Vector3 dirToTarget = (lockOnTarget.position - camera.transform.position).normalized;
        Vector3 rotToTarget = Quaternion.LookRotation(dirToTarget).eulerAngles;

        xRot = Mathf.LerpAngle(xRot, rotToTarget.x, walkSpeed * Time.deltaTime);
        camera.transform.localEulerAngles = new Vector3(xRot, 0, 0);

        yRot = Mathf.LerpAngle(yRot, rotToTarget.y, walkSpeed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(0, yRot, 0);


    }

    void CombatControls()
    {
        actionVector = actions.Look.ReadValue<Vector2>().normalized;
        if (actions.Attack.IsPressed())
        {

            if (actionVector.magnitude > 0)
            {
                atkAngle = Mathf.Atan2(actionVector.x, -actionVector.y) * 180 / Mathf.PI;
            }
        }
        if (actions.Defend.IsPressed())
        {
            if (actionVector.magnitude > 0)
            {
                animator.SetFloat("x", actionVector.x);
                animator.SetFloat("y", actionVector.y);
            }
        }
    }

    //Animation Events
    public void StartAttack()
    {
        animator.SetFloat("x", 0);
        animator.SetFloat("y", 0);
        weapon.SetState(PlayerWeapon.WeaponState.ATTACKING);
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
    }

    public void EndAttack()
    {
        weapon.SetState(PlayerWeapon.WeaponState.IDLE);
        if(GameSettings.slowCameraMovementWhenAttacking) armPivot.localEulerAngles = new Vector3(0, 0, 0);
        lookSpeed = GameSettings.aimSensitivity;
        if (!lockedOn) lockOnTarget = null;
    }

    public void Recoil()
    {
        weapon.SetState(PlayerWeapon.WeaponState.IDLE);
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
    }
}
