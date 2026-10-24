using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    public float lookSpeed = 100;
    public float moveSpeed = 10;
    public CharacterController controller;
    public Camera camera;


    float rotx = 0;
    float roty = 0;
    void Start()
    {
        
    }

    void FreeLook()
    {
        //Looking Around
        Vector2 lookInput = Game.input.Player.Look.ReadValue<Vector2>();
        rotx -= lookInput.y * lookSpeed * Time.deltaTime;
        rotx = Mathf.Clamp(rotx,-90,90);
        roty += lookInput.x * lookSpeed * Time.deltaTime;
        camera.transform.localEulerAngles = new Vector3(rotx,0,0);
        transform.localEulerAngles = new Vector3(0,roty,0);
    }

    void Movement()
    {
        float xMoveInput = Game.input.Player.Move.ReadValue<Vector2>().x;    
        float zMoveInput = Game.input.Player.Move.ReadValue<Vector2>().y;
        float magnitude = Game.input.Player.Move.ReadValue<Vector2>().magnitude;
        Vector3 moveDirection = ((camera.transform.right * xMoveInput) + (camera.transform.forward * zMoveInput)).normalized * magnitude;
        if(Game.input.Player.Jump.IsPressed()) moveDirection += transform.up;
        else if(Game.input.Player.Crouch.IsPressed()) moveDirection -= transform.up;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    void Update()
    {
        FreeLook();
        Movement();
    }
}
