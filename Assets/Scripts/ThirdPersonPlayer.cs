using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class ThirdPersonPlayer : MonoBehaviour
{
    public float speed = 5;
    public float sensitivity = 100f;
    public Transform camera;

    private Controls input; 
    private CharacterController controller;
    private Vector3 motion;

    void Start()
    {
        input = new Controls();
        input.Player.Enable();
        controller = GetComponent<CharacterController>();
    }
       
    void Update()
    {
        Movement();
    }

    void Movement()
    {
        float x = input.Player.Move.ReadValue<Vector2>().x;
        float z = input.Player.Move.ReadValue<Vector2>().y;
        motion = new Vector3(x, 0, z).normalized;

        if(motion.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(motion.x, motion.z) * Mathf.Rad2Deg + camera.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

    }
}
