using UnityEngine;

public class Ability_WallRun : MonoBehaviour
{
    public CharacterControls character;
    [SerializeField] float jumpForce = 5;
    [SerializeField] float wallDistance = 2;
    [SerializeField][Range(0,45)] float maxCameraTilt = 35; 
    [SerializeField] float cameraTiltSpeed = 10;
    [SerializeField] LayerMask wallLayer;
    
    RaycastHit wallHit;
    float cameraTilt = 0;
    bool canWallRun = false;
    bool isWallRunning = false;
    bool canWallJump = false;

    void Start()
    {
        if(!character) character = GetComponent<CharacterControls>();
    }
    void FixedUpdate()
    {
        Ray rayleft = new Ray(transform.position, -transform.right);
        Ray rayRight = new Ray(transform.position, transform.right);
        canWallRun = Physics.Raycast(rayleft, out wallHit, wallDistance,wallLayer) || Physics.Raycast(rayRight, out wallHit, wallDistance, wallLayer);
    }
    void Update()
    {
        //Conditions for Wall Run
        if(canWallRun && !isWallRunning && !character.grounded && Game.controls.Player.Jump.WasPerformedThisFrame())
        {
            StartWallRun();
            character.numJumps--;
        }

        //Wall Running Behaviour
        if(isWallRunning)
        {
            //Movement
            Vector3 wallNormal = wallHit.normal;
            Vector3 wallForward = Vector3.Cross(wallNormal,transform.up);
            if(Vector3.Dot(transform.forward,wallForward) < 0)
            {
                wallForward = -wallForward;
            }
            character.controller.Move((wallForward + new Vector3(0,character.camera.forward.y,0)).normalized * character.speed * Time.deltaTime);

            //Tilt Camera
            if(Vector3.Dot(wallNormal,transform.right) > Vector3.Dot(wallNormal,-transform.right))
            {
                cameraTilt = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, -maxCameraTilt, cameraTiltSpeed * Time.deltaTime);
            }
            else
            {
                cameraTilt = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, maxCameraTilt, cameraTiltSpeed * Time.deltaTime);
            }

            //Wall Jumping
            if(Game.controls.Player.Jump.WasPerformedThisFrame() && canWallJump)
            {
                EndWallRun();
                character.velocity = (wallHit.normal + Vector3.up).normalized * Mathf.Sqrt(jumpForce * 2 * 10);
            }
            //Cancel Wall Run
            if(!canWallRun || character.grounded)
            {
                EndWallRun();
            }
            
            canWallJump = true;
        }
        else
        {
            cameraTilt = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, 0, cameraTiltSpeed * Time.deltaTime);
        }

        Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, cameraTilt);
    }


    void StartWallRun()
    {
        canWallJump = false;
        character.gravityOn = false;
        isWallRunning = true;
    }

    void EndWallRun()
    {
        character.gravityOn = true;
        isWallRunning = false;
    }
}
