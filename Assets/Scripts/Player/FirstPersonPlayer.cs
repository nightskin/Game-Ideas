using UnityEngine;

public class FirstPersonPlayer : MonoBehaviour
{
    // For Input
    Controls controls;
    public Controls.PlayerActions actions;

    //Components
    public Transform camera;
    [SerializeField] CharacterController controller;

    // For basic motion
    public Vector3 moveDirection;
    public float moveSpeed = 15;

    //For Look/Aim
    public float lookSpeed = 100;
    float xRot = 0;
    float yRot = 0;

    // For Jumping
    [SerializeField] float jumpHeight = 5;
    [SerializeField] Vector3 velocity = new Vector3();
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask wallMask;
    Transform groundCheck;
    float gravity = -9.81f;
    private bool grounded;


    //For Dashing
    bool dashing = false;
    float dashSpeed = 30;
    float dashAmount = 0.1f;
    float dashTimer = 0;


    //For lockOn System
    Transform lockOnTarget = null;

    void Awake()
    {
        dashTimer = dashAmount;
        if(!groundCheck) groundCheck = transform.Find("GroundCheck");
        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();


        actions.Jump.performed += Jump_performed;
        actions.Dash.performed += Dash_performed;
        actions.LockOn.performed += LockOn_performed;
        actions.LockOn.canceled += LockOn_canceled;
    }


    void Update()
    {
        CanJump();

        if (!lockOnTarget)
        {
            Look();
        }
        else if (lockOnTarget)
        {
            Quaternion targetRot = Quaternion.LookRotation(lockOnTarget.position - camera.transform.position);
            xRot = targetRot.eulerAngles.x;
            yRot = targetRot.eulerAngles.y;
            camera.localEulerAngles = new Vector3(xRot, 0, 0);
            transform.localEulerAngles = new Vector3(0, yRot, 0);
        }

        if (dashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer < 0)
            {
                dashTimer = dashAmount;
                dashing = false;
            }
            else
            {
                Dash();
            }
        }
        else
        {
            if (lockOnTarget) LockOnMovement();
            else NormalMovement();
        }

    }

    void OnDestroy()
    {
        actions.Jump.performed -= Jump_performed;
        actions.LockOn.performed -= LockOn_performed;
        actions.LockOn.canceled -= LockOn_canceled;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Pickup")
        {

        }
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
    }

    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        dashing = true;
    }

    private void LockOn_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (Physics.Raycast(camera.position, camera.forward, out RaycastHit hit))
        {
            if (hit.transform.gameObject.layer == 6)
            {
                lockOnTarget = hit.transform;
            }
        }
    }

    private void LockOn_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        lockOnTarget = null;
    }


    void Dash()
    {
        if (grounded && velocity.y < 0)
        {
            velocity = Vector3.zero;
        }

        controller.Move(moveDirection * dashSpeed * Time.deltaTime);

        //Add Gravity
        velocity += Vector3.up * gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void NormalMovement()
    {
        if(grounded && velocity.y < 0)
        {
            velocity = Vector3.zero;
        }

        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = transform.right * x + transform.forward * z;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);


        //Add Gravity
        velocity += Vector3.up * gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }

    void LockOnMovement()
    {
        if (grounded && velocity.y < 0)
        {
            velocity = Vector3.zero;
        }

        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;


        moveDirection = camera.transform.right * x + camera.transform.forward * z;
        controller.Move(new Vector3(moveDirection.x, 0, moveDirection.z) * moveSpeed * Time.deltaTime);


        //Add Gravity
        velocity += Vector3.up * gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }

    void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;
        //Looking up/down with camera
        xRot -= y * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90, 45);
        camera.localEulerAngles = new Vector3(xRot, 0, 0);
        //Looking left right with player body
        yRot += x * lookSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }

    void CanJump()
    {
        grounded = Physics.Raycast(groundCheck.position, Vector3.down, 0.25f, groundMask);
        
    }
    
}
