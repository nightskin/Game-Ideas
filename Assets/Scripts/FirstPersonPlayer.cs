using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonPlayer : MonoBehaviour
{
    private float speed;
    private Vector3 velocity;

    public float jumpHeight = 3;
    public float normalSpeed = 5;
    public float dodgeSpeed = 10;
    public float sensitivity = 100f;

    public Transform camera;

    public Controls controls;
    public Controls.PlayerActions actions;
    private CharacterController controller;
    private Vector3 motion;



    private float gravity = -9.8f;
    private float xRot = 0;
    private float yRot = 0;

    void Awake()
    {
        speed = normalSpeed;
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Look();
        Movement();

        //Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        //Reset velocity when grounded
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }
        //Jumping
        if (actions.Jump.IsPressed() && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
    }

    void Movement()
    {
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        motion = transform.right * x + transform.forward * z;
        controller.Move(motion * speed * Time.deltaTime);

        
    }

    void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;
        //Looking up/down with camera
        xRot -= y * sensitivity * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -45, 45);
        camera.localEulerAngles = new Vector3(xRot, 0, 0);
        //Looking left right with player body
        yRot += x * sensitivity * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }

}
