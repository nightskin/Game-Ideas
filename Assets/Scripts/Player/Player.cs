using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

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

    //For Basic Controls
    [Header("General")]
    float moveSpd;
    [HideInInspector] public float lookSpeed;
    [SerializeField][Min(1)] float walkSpeed = 25;
    [SerializeField][Min(2)] float runSpeed = 50;
    [HideInInspector] public bool isCrouching = false;
    [SerializeField] float crouchSpeed = 5;
    [HideInInspector] public Vector3 velocity = Vector3.zero;

    RaycastHit slopeHit;
    float xRot = 0;
    float yRot = 0;
    Vector3 moveDirection;


    //For Combat
    [HideInInspector] public bool isAttacking = false;
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
    [SerializeField][Min(0)] int maxJumps = 1;
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

    //LockOn System
    [Header("LockOnSystem")]
    [SerializeField] bool lockOnSystemEnabled = true;
    [SerializeField] float lockOnDistance = 500;
    [SerializeField] LayerMask lockOnLayer;
    [HideInInspector] public Transform lockOnTarget = null;
    float lockOnLerp = 0;

    void Start()
    {
        lookSpeed = Game.aimSense;
        moveSpd = walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;
        stunTimer = stunTime;


        Game.controls.Player.Jump.performed += Jump_performed;
        Game.controls.Player.LockOn.performed += LockOn_performed;
        Game.controls.Player.Crouch.performed += Crouch_performed;
        Game.controls.Player.Sprint.performed += Sprint_performed;
        Game.controls.Player.Sprint.canceled += Sprint_canceled;
        Game.controls.Player.Attack.performed += Attack_performed;
        Game.controls.Player.Defend.performed += Defend_performed;
        Game.controls.Player.Defend.canceled += Defend_canceled;

    }

    void Update()
    {
        if (lockOnTarget != null)
        {
            LockOnMovement();
        }
        else
        {
            FPSMovement();
        }


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
        Game.controls.Player.LockOn.performed -= LockOn_performed;
        Game.controls.Player.Crouch.performed -= Crouch_performed;
        Game.controls.Player.Sprint.performed -= Sprint_performed;
        Game.controls.Player.Sprint.canceled -= Sprint_canceled;
        Game.controls.Player.Attack.performed -= Attack_performed;
        Game.controls.Player.Defend.performed -= Defend_performed;
        Game.controls.Player.Defend.canceled -= Defend_canceled;
    }
    
    private void Jump_performed(InputAction.CallbackContext obj)
    {
        // Normal Jump
        if (jumpingEnabled)
        {
            if (numberOfJumps < maxJumps)
            {
                velocity = Vector3.up * Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
                numberOfJumps++;
                jumping = true;
                return;
            }
        }
    }

    private void LockOn_performed(InputAction.CallbackContext obj)
    {
        if (lockOnSystemEnabled)
        {
            if (lockOnTarget == null)
            {
                if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, lockOnDistance, lockOnLayer))
                {
                    hud.animator.SetBool("lock", true);
                    lockOnTarget = hit.transform;
                }
            }
            else
            {
                lockOnTarget = null;
                hud.animator.SetBool("lock", false);
                lockOnLerp = 0;
            }
        }
    }

    private void Crouch_performed(InputAction.CallbackContext obj)
    {
        if (grounded)
        {
            if (isCrouching)
            {
                camera.transform.localPosition = new Vector3(0, 1, 0);
                controller.center = new Vector3(0, 0.5f, 0);
                controller.height = 1;
                isCrouching = false;
                moveSpd = walkSpeed;
            }
            else
            {
                camera.transform.localPosition = new Vector3(0, 2, 0);
                controller.center = new Vector3(0, 1, 0);
                controller.height = 2;
                isCrouching = true;
                moveSpd = crouchSpeed;
            }
        }
    }

    private void Sprint_performed(InputAction.CallbackContext obj)
    {
        moveSpd = runSpeed;
    }

    private void Sprint_canceled(InputAction.CallbackContext obj)
    {
        moveSpd = walkSpeed;
    }

    private void Attack_performed(InputAction.CallbackContext obj)
    {
        lookSpeed *= Game.slowCameraAtkAmount;
        animator.SetTrigger("slash");
    }

    private void Defend_performed(InputAction.CallbackContext obj)
    {
        if(weapon.size == Weapon.WeaponSize.SMALL)
        {
            if (!dashing)
            {
                dashDirection = moveDirection.normalized;
                dashing = true;
                dashTimer = dashTime;
            }
        }
        else
        {
            lookSpeed *= Game.slowCameraDefAmont;
            animator.SetBool("blocking", true);
        }
    }

    private void Defend_canceled(InputAction.CallbackContext obj)
    {
        lookSpeed = Game.aimSense;
        if(weapon.size != Weapon.WeaponSize.SMALL)
        {
            animator.SetBool("blocking", false);
            defVector = Vector2.zero;
            animator.SetFloat("x", defVector.x);
            animator.SetFloat("y", defVector.y);
        }
    }


    void Combat()
    {
        if (wasHit)
        {

        }
        else
        {
            actionVector = Game.controls.Player.Look.ReadValue<Vector2>().normalized;
            if (Game.controls.Player.Attack.IsPressed())
            {
                atkAngle = Mathf.Atan2(actionVector.x, -actionVector.y) * 180 / Mathf.PI;
            }
            else if (Game.controls.Player.Defend.IsPressed())
            {
                defVector.y += actionVector.y * 20 * Time.deltaTime;
                defVector.y = Mathf.Clamp(defVector.y, 0, 1);
                animator.SetFloat("y", defVector.y);
                
                defVector.x += actionVector.x * 20 * Time.deltaTime;
                defVector.x = Mathf.Clamp(defVector.x, -1, 1);
                animator.SetFloat("x", defVector.x);
            }
        }
    }
    void FPSMovement()
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
            controller.Move(moveDirection * moveSpd * Time.deltaTime);
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
    void LockOnMovement()
    {
        //Look At Target
        Vector3 dirToTarget = lockOnTarget.position - camera.transform.position;
        Vector3 rotToTarget = Quaternion.LookRotation(dirToTarget).eulerAngles;


        if (lockOnLerp < 1)
        {
            lockOnLerp += Time.deltaTime;
            xRot = Mathf.LerpAngle(xRot, rotToTarget.x, lockOnLerp);
            yRot = Mathf.LerpAngle(yRot, rotToTarget.y, lockOnLerp);
        }
        else
        {
            xRot = rotToTarget.x;
            if (xRot > 359) xRot = 0;
            yRot = rotToTarget.y;
        }

        camera.transform.localRotation = Quaternion.Euler(xRot, 0, 0);
        transform.rotation = Quaternion.Euler(0, yRot, 0);

        //Moving Around
        if (grounded && velocity.y < 0)
        {
            numberOfJumps = 0;
            velocity = Vector3.zero;
            if (jumping) jumping = false;
        }

        float x = Game.controls.Player.Move.ReadValue<Vector2>().x;
        float z = Game.controls.Player.Move.ReadValue<Vector2>().y;
        float m = Game.controls.Player.Move.ReadValue<Vector2>().magnitude;
        moveDirection = (transform.right * x + transform.forward * z).normalized * m;

        //Animation for moving
        if (m > 0)
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
            controller.Move(moveDirection * moveSpd * Time.deltaTime);
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
    public void StartSlash()
    {
        isAttacking = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        StartCoroutine(weapon.AnimateTrail());
        defVector = Vector2.zero;
        animator.SetFloat("x", defVector.x);
        animator.SetFloat("y", defVector.y);
    }
    public void EndSlash()
    {
        isAttacking = false;
        lookSpeed = Game.aimSense;
        armPivot.localEulerAngles = Vector3.zero;
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
        isAttacking = false;
        armPivot.localEulerAngles = Vector3.zero;
    }
    public void BackToIdle()
    {
        isAttacking = false;
        defVector = Vector2.zero;
        animator.SetFloat("x", defVector.x);
        animator.SetFloat("y", defVector.y);
    }

}
