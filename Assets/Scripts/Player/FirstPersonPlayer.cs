using UnityEngine;
using UnityEngine.UI;

public class FirstPersonPlayer : MonoBehaviour
{
    // For Input
    Controls controls;
    public Controls.PlayerActions actions;
    
    //Components
    public Transform camera;
    [SerializeField] CharacterController controller;

    // For basic motion
    Vector3 motion;
    public float moveSpeed = 15;

    //For Look/Aim
    public Vector2 lookSpeed = new Vector2(100f, 100);
    float xRot = 0;
    float yRot = 0;

    // For Jumping
    [SerializeField] float jumpHeight = 5;
    [SerializeField] Vector3 velocity = new Vector3();
    [SerializeField] LayerMask groundMask;
    Transform groundCheck;
    float gravity = -9.81f;
    private bool isGrounded;

    //For Dashing
    [SerializeField] float dashSpeed = 100;
    [SerializeField] float dashTime = 0.1f;
    Vector3 dashDirection;
    float dashTimer;
    bool dashing;


    void Awake()
    {
        dashing = false;
        dashTimer = dashTime;
        if(!groundCheck) groundCheck = transform.Find("GroundCheck");
        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();
        actions.Dash.performed += Dash_performed;
        actions.Jump.performed += Jump_performed;
    }



    private void OnDestroy()
    {
        actions.Dash.performed -= Dash_performed;
    }

    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(isGrounded) velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
    }


    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        dashDirection = motion;
        dashing = true;
    }



    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.25f,groundMask);

        Look();

        if (dashing)
        {
            dashTimer -= Time.deltaTime;
            if(dashTimer <= 0)
            {
                dashTimer = dashTime;
                dashing = false;
            }
            else
            {
                controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            }
        }
        else
        {
            Movement();
        }




    }

    void Movement()
    {
        if(isGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        motion = transform.right * x + transform.forward * z;
        controller.Move(motion * moveSpeed * Time.deltaTime);

        //Gravity
        velocity += Vector3.up * gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }

    void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;
        //Looking up/down with camera
        xRot -= y * lookSpeed.y * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90, 45);
        camera.localEulerAngles = new Vector3(xRot, 0, 0);
        //Looking left right with player body
        yRot += x * lookSpeed.x * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }


}
