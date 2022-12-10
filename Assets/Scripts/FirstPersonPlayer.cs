using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonPlayer : MonoBehaviour
{
    [SerializeField] private Transform camera;
    [SerializeField] private CharacterController controller;
    public Controls.PlayerActions actions;
    public Vector2 lookSpeed = new Vector2(100f, 100);
    public float moveSpd = 10;


    private Vector3 velocity;

    private Controls controls;
    private Vector3 motion;

    private float xRot = 0;
    private float yRot = 0;

    void Awake()
    {
        controls = new Controls();
        actions = controls.Player;
        actions.Enable();
    }

    void Update()
    {
        Look();
        Movement();
    }

    void Movement()
    {
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        motion = transform.right * x + transform.forward * z;
        controller.Move(motion * moveSpd * Time.deltaTime);
        
    }

    void Look()
    {
        float x = actions.Look.ReadValue<Vector2>().x;
        float y = actions.Look.ReadValue<Vector2>().y;
        //Looking up/down with camera
        xRot -= y * lookSpeed.y * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -45, 45);
        camera.localEulerAngles = new Vector3(xRot, 0, 0);
        //Looking left right with player body
        yRot += x * lookSpeed.x * Time.deltaTime;
        transform.localEulerAngles = new Vector3(0, yRot, 0);
    }

}
