using System;
using System.Collections;
using UnityEngine;

public class Mage : MonoBehaviour
{
    //Components
    [Header("Components")]
    public Camera camera;
    public CharacterController controller;
    public Animator animator;
    public Status status;
    
    //States
    MageState currentState;
    public MageOK ok = new MageOK();
    public MageHIT hit = new MageHIT();
    public MageDEAD dead = new MageDEAD();

    //looking Around
    float xRot = 0;
    float yRot = 0;
    [SerializeField] float maxHeadTurn = 65;

    //moving around
    Vector3 moveDirection = Vector3.zero;
    public Vector3 velocity = Vector3.zero;
    bool jumping = false;
    [HideInInspector] public bool grounded = false;
    RaycastHit slopeHit;
    float groundDistance = 1;
    float speed;

    [SerializeField] float minSpeed = 10;
    [SerializeField] float maxSpeed = 20;
    [SerializeField] float drag = 25;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float jumpHeight = 3;


    void Start()
    {
        if(!controller) controller = GetComponent<CharacterController>();
        if(!camera) camera = Camera.main;
        if(!animator) animator = GetComponent<Animator>();

        speed = minSpeed;
        SwtichState(ok);
    }

    void Update()
    {
        //checks if player is grounded or not
        Ray groundRay = new Ray(transform.position, Vector3.down);
        grounded = Physics.Raycast(groundRay, out slopeHit, groundDistance, groundLayer);
        
        currentState.Update(this);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        currentState.Collision(this);
    }

    public void SwtichState(MageState newState)
    {
        currentState = newState;
        currentState.Enter(this);

        if(currentState == hit)
        {
            Debug.Log("You Got Hit NOOB");
        }
        else if(currentState == dead)
        {
            Debug.Log("YOU DEAD");
        }
    }

    public void Move()
    {
        // When player hits the ground
        if (grounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        //Dash when Pressed
        if(Game.controls.Player.Sprint.IsPressed())
        {
            speed = maxSpeed;
        }
        else
        {
            speed = minSpeed;
        }

        // Moving Around 
        float x = Game.controls.Player.Move.ReadValue<Vector2>().x;
        float z = Game.controls.Player.Move.ReadValue<Vector2>().y;
        moveDirection = (transform.right * x + transform.forward * z).normalized;
        controller.Move(moveDirection * speed * Time.deltaTime);

        //Apply Forces
        velocity.y += -10 * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //Jumping
        if(Game.controls.Player.Jump.IsPressed() && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
            jumping = true;
        }
        else
        {
            jumping = false;
        }

        //Handle Moving Down slopes
        if (grounded && !jumping)
        {
            controller.Move(new Vector3(0, -slopeHit.distance, 0));
        }
    }

    public void Look()
    {
        //Looking Around
        float lx = Game.controls.Player.Look.ReadValue<Vector2>().x;
        float ly = Game.controls.Player.Look.ReadValue<Vector2>().y;

        // Looking up/down with camera
        xRot -= ly * Game.aimSense * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -maxHeadTurn, maxHeadTurn);
        camera.transform.localRotation = Quaternion.Euler(xRot, 0, 0);

        // Looking left right with player body
        yRot += lx * Game.aimSense * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, yRot, 0);
    }

}
