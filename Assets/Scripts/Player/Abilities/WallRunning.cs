using UnityEngine;
using System;

[System.Serializable]
public class WallRunning : PlayerAbility
{    
    [Header("Wall Movement")]
    [SerializeField][Range(0,90)] float cameraTiltAngleWhileWallRunning = 35; 
    [SerializeField] float cameraTiltSpeed = 10;
    [SerializeField] bool allowPlayerToJumpAgainAfterWallRun = true;
    [SerializeField] float wallDistance = 1;
    RaycastHit wallHit;
    
    
    float cameraTilt = 0;
    bool canTakenExtraJump;
    bool isWallRunning = false;
    bool canWallRun = false;
    bool canWallJump = false;

    public override void Init()
    {
        
    }

    public override void FixedUpdate()
    {
        Ray rayleft = new Ray(owner.transform.position, -owner.transform.right);
        Ray rayRight = new Ray(owner.transform.position, owner.transform.right);
        canWallRun = Physics.Raycast(rayleft, out wallHit, wallDistance) || Physics.Raycast(rayRight, out wallHit, wallDistance);
    }

    public override void Update()
    {
        //Conditions for Wall Run
        if(canWallRun && !isWallRunning && !owner.onGround && Game.input.Player.Jump.WasPerformedThisFrame())
        {
            if(allowPlayerToJumpAgainAfterWallRun) canTakenExtraJump = true;
            canWallJump = false;
            owner.gravityOn = false;
            isWallRunning = true;
        }


        if(isWallRunning)
        {
            //Wall Running Movement
            Vector3 wallNormal = wallHit.normal;
            Vector3 wallForward = Vector3.Cross(wallNormal,owner.transform.up);
            if(Vector3.Dot(owner.transform.forward,wallForward) < 0)
            {
                wallForward = -wallForward;
            }
            owner.controller.Move((wallForward + new Vector3(0,owner.cameraHolder.forward.y,0)).normalized * owner.currentSpeed * Time.deltaTime);

            //Tilt Camera
            if(Vector3.Dot(wallNormal,owner.transform.right) > Vector3.Dot(wallNormal,-owner.transform.right))
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
                owner.gravityOn = true;
                isWallRunning = false;
                owner.velocity = (wallHit.normal + Vector3.up).normalized * Mathf.Sqrt(owner.jumpHeight * 2 * 10);
            }
            //Cancel Wall Run
            if(!canWallRun || owner.onGround)
            {
                owner.gravityOn = true;
            isWallRunning = false;
            }
            
            canWallJump = true;
        }
        else
        {
            if(Game.input.Player.Move.ReadValue<Vector2>().magnitude > 0.5f)
            {
                owner.velocity = Vector3.Lerp(owner.velocity, new Vector3(0, owner.velocity.y, 0), Time.deltaTime);
            }
            if(canTakenExtraJump && Game.input.Player.Jump.WasPerformedThisFrame() && allowPlayerToJumpAgainAfterWallRun)
            {
                owner.velocity.y = Mathf.Sqrt(owner.jumpHeight * 2 * owner.gravityStrength);
                canTakenExtraJump = false;
            }
            cameraTilt = Mathf.LerpAngle(Camera.main.transform.localEulerAngles.z, 0, cameraTiltSpeed * Time.deltaTime);
        }

        Camera.main.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, cameraTilt);
    }

}
