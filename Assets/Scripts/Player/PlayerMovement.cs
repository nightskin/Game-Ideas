using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Components
    [Header("Components")]
    public PlayerHUD hud;
    public Camera camera;
    public CharacterController controller;
    public PlayerCombatControls combatControls;


    //For Basic Controls
    [Header("General")]
    float moveSpeed;
    [SerializeField][Min(1)] float walkSpeed = 25;
    [SerializeField][Min(2)] float runSpeed = 50;
    [HideInInspector] public bool isCrouching = false;
    [SerializeField] float crouchSpeed = 5;
    [HideInInspector] public float lookSpeed;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [SerializeField][Min(0)] float cameraBobSpeed = 1f;
    RaycastHit slopeHit;
    float xRot = 0;
    float yRot = 0;
    Vector3 moveDirection;
    
    // For Jumping Around
    [SerializeField] LayerMask groundLayer;
    [SerializeField][Min(0)] float groundDistance = 0.5f;
    [HideInInspector] public bool grounded;
    bool jumping = false;
    [SerializeField] bool jumpingEnabled;
    [SerializeField][Min(0)] int maxJumps = 1;
    [SerializeField][Min(1)] float jumpHeight = 3;


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


    [Header("Wall Movement")]
    [SerializeField] bool wallRunningEnabled = false;
    [SerializeField] float maxWallRunTime = 5;
    [SerializeField] float wallRunSpeed = 30;
    [SerializeField] float wallJumpForce = 5;
    [SerializeField] float maxCameraTiltAngle = 35;
    [SerializeField] float cameraTiltSpeed = 5;
    [SerializeField][Min(0)] float wallDistance = 0.7f;

    float wallRunCamTiltTime;
    float wallRunTimer = 0;
    int numberOfJumps = 0;
    bool isWallRunning = false;
    bool isAgainstWall = false;
    RaycastHit wallHit;

    void Start()
    {
        moveSpeed = walkSpeed;
        lookSpeed = Game.mouseSensitivity;

        Cursor.lockState = CursorLockMode.Locked;

        Game.controls.Player.Jump.performed += Jump_performed;
        Game.controls.Player.Sprint.performed += Sprint_performed;
        Game.controls.Player.Sprint.canceled += Sprint_canceled;
        Game.controls.Player.Dash.performed += Dash_performed;
        Game.controls.Player.LockOn.performed += LockOn_performed;
        Game.controls.Player.Crouch.performed += Crouch_performed;
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

        if (combatControls.stunned)
        {
            StunKnockBack();
        }
        else
        {
            if (isWallRunning)
            {
                WallRun();
            }
            else
            {
                NormalMovement();
            }
        }


    }

    void FixedUpdate()
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        grounded = Physics.Raycast(groundRay, out slopeHit, groundDistance, groundLayer);
        
        if (wallRunningEnabled)
        {
            Ray wallRayLeft = new Ray(transform.position, -transform.right);
            Ray wallRayRight = new Ray(transform.position, transform.right);
            isAgainstWall = Physics.Raycast(wallRayLeft, out wallHit, wallDistance) || Physics.Raycast(wallRayRight, out wallHit, wallDistance);
        }
        
    }

    void OnDestroy()
    {
        Game.controls.Player.Jump.performed -= Jump_performed;
        Game.controls.Player.Sprint.performed -= Sprint_performed;
        Game.controls.Player.Sprint.canceled -= Sprint_canceled;
        Game.controls.Player.Dash.performed -= Dash_performed;
        Game.controls.Player.LockOn.performed -= LockOn_performed;
        Game.controls.Player.Crouch.performed -= Crouch_performed;
    }

    private void Jump_performed(InputAction.CallbackContext obj)
    {
        if (isWallRunning)
        {
            //Wall jump
            velocity = (wallHit.normal + Vector3.up).normalized * Mathf.Sqrt(wallJumpForce * -2 * Physics.gravity.y);
            isWallRunning = false;
            return;
        }
        else
        {
            // Start Wall Run
            if (isAgainstWall && !grounded)
            {
                wallRunTimer = maxWallRunTime;
                wallRunCamTiltTime = 0;
                isWallRunning = true;
                numberOfJumps--;
                return;
            }
            // Normal Jump
            if (jumpingEnabled)
            {
                if (numberOfJumps < maxJumps)
                {
                    velocity = Vector3.up * Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
                    jumping = true;
                    numberOfJumps++;
                    return;
                }
            }
        }
    }

    private void Dash_performed(InputAction.CallbackContext obj)
    {
        if (!dashing)
        {
            dashDirection = moveDirection.normalized;
            dashing = true;
            dashTimer = dashTime;
        }
    }

    private void Sprint_performed(InputAction.CallbackContext obj)
    {
        moveSpeed = runSpeed;
    }

    private void Sprint_canceled(InputAction.CallbackContext obj)
    {
        moveSpeed = walkSpeed;
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
                Crouch(true);
            }
            else
            {
                Crouch(false);
            }
        }
    }

    public void Crouch(bool activated)
    {
        if (activated)
        {
            camera.transform.localPosition = new Vector3(0, 1, 0);
            controller.center = new Vector3(0, 0.5f, 0);
            controller.height = 1;
            isCrouching = false;
            moveSpeed = walkSpeed;
        }
        else
        {
            camera.transform.localPosition = new Vector3(0, 2, 0);
            controller.center = new Vector3(0, 1, 0);
            controller.height = 2;
            isCrouching = true;
            moveSpeed = crouchSpeed;
        }
    }

    void StunKnockBack()
    {
        TiltCamera(0);
        if (grounded && velocity.y < 0)
        {
            numberOfJumps = 0;
            velocity = Vector3.zero;
            jumping = false;
        }

        controller.Move(combatControls.knockBackForce * Time.deltaTime);
        
        //Gravity
        velocity += new Vector3(0, -10, 0) * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        combatControls.StunCountDown();
    }

    void NormalMovement()
    {
        TiltCamera(0);
        if (grounded && velocity.y < 0)
        {
            numberOfJumps = 0;
            velocity = Vector3.zero;
            jumping = false;
        }

        float x = Game.controls.Player.Move.ReadValue<Vector2>().x;
        float z = Game.controls.Player.Move.ReadValue<Vector2>().y;
        float m = Game.controls.Player.Move.ReadValue<Vector2>().magnitude;
        moveDirection = (transform.right * x + transform.forward * z).normalized * m;

        //Move Input
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

        //Camera Bob
        if (Game.cameraBob)
        {
            if (moveDirection.magnitude > 0 && grounded)
            {
                camera.transform.localPosition = new Vector3(0, controller.height + Mathf.PingPong(Time.time * cameraBobSpeed, 0.5f), 0);
            }
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

    void WallRun()
    {
        float x = Game.controls.Player.Move.ReadValue<Vector2>().x;
        float z = Game.controls.Player.Move.ReadValue<Vector2>().y;

        moveDirection = transform.right * x + transform.forward * z;

        Vector3 wallNormal = wallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((transform.forward - wallForward).magnitude > (transform.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        controller.Move((wallForward + new Vector3(0, camera.transform.forward.y, 0)).normalized * wallRunSpeed * Time.deltaTime);
        wallRunTimer -= Time.deltaTime;

        if (grounded || !isAgainstWall || Vector3.Dot(moveDirection, -wallNormal) < 0 || wallRunTimer <= 0)
        {
            isWallRunning = false;
        }

        TiltCamera(Vector3.Dot(wallNormal, transform.right) * maxCameraTiltAngle * -1);

    }

    void TiltCamera(float amount)
    {
        if (wallRunCamTiltTime < 1)
        {
            float z = Mathf.LerpAngle(camera.transform.localEulerAngles.z, amount, wallRunCamTiltTime);
            wallRunCamTiltTime += cameraTiltSpeed * Time.deltaTime;
            camera.transform.localEulerAngles = new Vector3(camera.transform.localEulerAngles.x, camera.transform.localEulerAngles.y, z);
        }
        else
        {
            camera.transform.localEulerAngles = new Vector3(camera.transform.localEulerAngles.x, camera.transform.localEulerAngles.y, amount);
        }

    }

    void LookAround()
    {
        float x = Game.controls.Player.Look.ReadValue<Vector2>().x;
        float y = Game.controls.Player.Look.ReadValue<Vector2>().y;

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
            yRot = rotToTarget.y;
        }
        
        camera.transform.localEulerAngles = new Vector3(xRot, 0, 0);
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }

}
