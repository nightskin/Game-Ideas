using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flight : MonoBehaviour
{
    public float speed = 5;
    public float sensitivityY = 100f;
    public float sensitivityX = 100f;
    public Transform camera;

    public Controls controls;
    public Controls.PlayerActions actions;
    private CharacterController controller;
    private Vector3 motion;

    private float xRot = 0;
    private float yRot = 0;
    
    void Start()
    {
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
    }

    void Look()
    {
        float LookX = actions.Look.ReadValue<Vector2>().x;
        float LookY = actions.Look.ReadValue<Vector2>().y;
        //Looking up/down with camera
        xRot -= LookY * sensitivityX * Time.deltaTime;
        yRot += LookX * sensitivityY * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -45, 45);
        camera.localEulerAngles = new Vector3(xRot, yRot, 0);
    }

    void Movement()
    {
        float x = actions.Move.ReadValue<Vector2>().x;
        float z = actions.Move.ReadValue<Vector2>().y;

        motion = camera.transform.right * x + camera.transform.forward * z;
        controller.Move(motion * speed * Time.deltaTime);
    }


}
