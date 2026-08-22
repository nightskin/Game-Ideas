using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallMovement : MonoBehaviour
{
    public CharacterMovement basicMovement;
    [SerializeField] float speed = 25;
    [SerializeField] float wallJumpForce = 5;
    [SerializeField] float wallDistance = 2;
    [SerializeField][Range(0,90)] float cameraTiltWhileWallRunning = 35; 
    [SerializeField] float cameraTiltSpeed = 10;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] bool allowPlayerToJumpAgainAfterWallRun = true;
    
    RaycastHit wallHit;
    float cameraTilt = 0;
    bool canTakenExtraJump;
    bool canWallRun = false;
    bool isWallRunning = false;
    bool canWallJump = false;

    void Start()
    {
        if(!basicMovement) basicMovement = GetComponent<CharacterMovement>();
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
        if(canWallRun && !isWallRunning && !basicMovement.onGround && Game.input.Player.Jump.WasPerformedThisFrame())
        {
            StartWallRun();
        }


        if(isWallRunning)
        {
            //Wall Running Movement
            Vector3 wallNormal = wallHit.normal;
            Vector3 wallForward = Vector3.Cross(wallNormal,transform.up);
            if(Vector3.Dot(transform.forward,wallForward) < 0)
            {
                wallForward = -wallForward;
            }
            basicMovement.controller.Move((wallForward + new Vector3(0,basicMovement.cameraHolder.forward.y,0)).normalized * speed * Time.deltaTime);

            //Tilt Camera
            if(Vector3.Dot(wallNormal,transform.right) > Vector3.Dot(wallNormal,-transform.right))
            {
                cameraTilt = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, -cameraTiltWhileWallRunning, cameraTiltSpeed * Time.deltaTime);
            }
            else
            {
                cameraTilt = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, cameraTiltWhileWallRunning, cameraTiltSpeed * Time.deltaTime);
            }

            //Wall Jumping
            if(Game.input.Player.Jump.WasPerformedThisFrame() && canWallJump)
            {
                EndWallRun();
                basicMovement.velocity = (wallHit.normal + Vector3.up).normalized * Mathf.Sqrt(wallJumpForce * 2 * 10);
            }
            //Cancel Wall Run
            if(!canWallRun || basicMovement.onGround)
            {
                EndWallRun();
            }
            
            canWallJump = true;
        }
        else
        {
            if(Game.input.Player.Move.ReadValue<Vector2>().magnitude > 0.5f)
            {
                StartCoroutine(ApplyDrag(1,0));
            }
            if(canTakenExtraJump && Game.input.Player.Jump.WasPerformedThisFrame() && allowPlayerToJumpAgainAfterWallRun)
            {
                basicMovement.velocity.y = Mathf.Sqrt(basicMovement.jumpHeight * 2 * basicMovement.gravityStrength);
                canTakenExtraJump = false;
            }
            cameraTilt = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, 0, cameraTiltSpeed * Time.deltaTime);
        }

        Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, cameraTilt);
    }

    IEnumerator ApplyDrag(float amount = 1 , float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        float t = 1;
        while(t > 0)
        {
            basicMovement.velocity = Vector3.Lerp(basicMovement.velocity, new Vector3(0, basicMovement.velocity.y, 0),t);
            t -= amount * Time.deltaTime;
            yield return null;
        }
    }

    void StartWallRun()
    {
        canTakenExtraJump = true;
        canWallJump = false;
        basicMovement.gravityOn = false;
        isWallRunning = true;
    }

    void EndWallRun()
    {
        basicMovement.gravityOn = true;
        isWallRunning = false;
    }
}
