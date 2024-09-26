using UnityEngine;

public class FirstPersonPlayer : MonoBehaviour
{
    Controls controls;
    public Controls.PlayerActions actions;

    //Components
    public Transform camera;
    public CharacterController controller;

    //For Basic Movement/Looking
    [SerializeField] float speed = 20;
    Vector3 moveDirection;

    public float lookSpeed = 100;

    float xRot = 0;
    float yRot = 0;
    float zRot = 0;

    // For Jumping and falling
    [SerializeField] float jumpHeight = 3;
    [SerializeField] LayerMask jumpLayer;
    [SerializeField] float gravity = 10.0f;

    bool isGrounded = false;
    Vector3 velocity = Vector3.zero;

    //Wall
    RaycastHit wallHit;
    bool isAgainstWall = false;
    
    void Awake()
    {
        if (!camera) camera = transform.Find("Camera");
        Cursor.lockState = CursorLockMode.Locked;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();

        actions.Jump.performed += Jump_performed;
        actions.Jump.canceled += Jump_canceled;
    }

    void Update()
    {
        Look();
        Movement();
    }

    void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(transform.position + Vector3.down, controller.radius, jumpLayer);
        isAgainstWall = Physics.Raycast(transform.position, moveDirection, out wallHit, 1, jumpLayer);


    }

    void OnDestroy()
    {
        actions.Jump.performed -= Jump_performed;
        actions.Jump.canceled -= Jump_canceled;
    }
    
    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            velocity = Vector3.up * Mathf.Sqrt(jumpHeight * 2 * gravity);
        }
    }

    private void Jump_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    
    void Movement()
    {
        if(isGrounded && velocity.magnitude > 0) 
        {
            velocity = Vector3.zero;
        }


        //Basic Motion
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        moveDirection = transform.right * x + transform.forward * z;
        controller.Move(moveDirection * speed * Time.deltaTime);

        velocity += -transform.up * gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;
        //Looking up/down with camera
        xRot -= y * lookSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90, 45);
        camera.localEulerAngles = new Vector3(xRot, camera.localEulerAngles.y, camera.localEulerAngles.z);
        //Looking left right with player body
        yRot += x * lookSpeed * Time.deltaTime;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, yRot, transform.localEulerAngles.z);

    }

}
