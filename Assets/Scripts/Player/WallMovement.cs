using System.Collections;
using UnityEngine;

public class WallMovement : MonoBehaviour
{
    public Player player;
    [SerializeField] float speed = 25;
    [SerializeField] float wallJumpForce = 5;
    [SerializeField] float wallDistance = 2;
    [SerializeField][Range(0,90)] float cameraTiltAngleWhileWallRunning = 35; 
    [SerializeField] float cameraTiltSpeed = 10;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] bool allowPlayerToJumpAgainAfterWallRun = true;
    
    RaycastHit wallHit;
    float cameraTilt = 0;
    bool canTakenExtraJump;
    bool isWallRunning = false;
    bool canWallRun = false;
    bool canWallJump = false;

    void Start()
    {
        if(!player) player = GetComponent<Player>();
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
        if(canWallRun && !isWallRunning && !player.onGround && Game.input.Player.Jump.WasPerformedThisFrame())
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
            player.controller.Move((wallForward + new Vector3(0,player.cameraHolder.forward.y,0)).normalized * speed * Time.deltaTime);

            //Tilt Camera
            if(Vector3.Dot(wallNormal,transform.right) > Vector3.Dot(wallNormal,-transform.right))
            {
                cameraTilt = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, -cameraTiltAngleWhileWallRunning, cameraTiltSpeed * Time.deltaTime);
            }
            else
            {
                cameraTilt = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, cameraTiltAngleWhileWallRunning, cameraTiltSpeed * Time.deltaTime);
            }

            //Wall Jumping
            if(Game.input.Player.Jump.WasPerformedThisFrame() && canWallJump)
            {
                EndWallRun();
                player.velocity = (wallHit.normal + Vector3.up).normalized * Mathf.Sqrt(wallJumpForce * 2 * 10);
            }
            //Cancel Wall Run
            if(!canWallRun || player.onGround)
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
                player.velocity.y = Mathf.Sqrt(player.jumpHeight * 2 * player.gravityStrength);
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
            player.velocity = Vector3.Lerp(player.velocity, new Vector3(0, player.velocity.y, 0),t);
            t -= amount * Time.deltaTime;
            yield return null;
        }
    }

    void StartWallRun()
    {
        canTakenExtraJump = true;
        canWallJump = false;
        player.gravityOn = false;
        isWallRunning = true;
    }

    void EndWallRun()
    {
        player.gravityOn = true;
        isWallRunning = false;
    }
}
