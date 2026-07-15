using UnityEngine;

public class WallRunning : MonoBehaviour
{
    float wallTilt = 0;
    bool wallRunning = false;
    bool canWallRun = false;
    public CharacterControls controls;
    [SerializeField] float wallRunSpeed = 50;
    [SerializeField] float jumpForce = 5;
    [SerializeField] float wallDistance = 2;
    [SerializeField] LayerMask wallLayer;
    RaycastHit wallHit;

    void Start()
    {
        if(!controls) controls = GetComponent<CharacterControls>();
    }

    void FixedUpdate()
    {
        Ray rightDir = new Ray (transform.position, transform.right);
        Ray leftDir = new Ray (transform.position, -transform.right);
        canWallRun = (Physics.Raycast(leftDir, out wallHit, wallDistance, wallLayer) || Physics.Raycast(rightDir, out wallHit, wallDistance, wallLayer)) && !controls.grounded;


        //Wall Tilting
        if(wallRunning)
        {
            if(Physics.Raycast(rightDir, out wallHit, wallDistance, wallLayer))
            {
                wallTilt = 45;
            }
            else if(Physics.Raycast(leftDir, out wallHit, wallDistance, wallLayer))
            {
                wallTilt = -45;
            }
        }
        else
        {
            wallTilt = 0;
        }
        
    }

    void Update()
    {
        // Camera Tilt for Wall Running
        controls.camera.localEulerAngles = Vector3.Lerp(controls.camera.localEulerAngles, new Vector3(controls.camera.localEulerAngles.x, controls.camera.localEulerAngles.y, wallTilt), 10 * Time.deltaTime);
        
        //Wall Run Movement
        if(wallRunning)
        {
            Vector3 wallNormal = wallHit.normal;
            Vector3 wallForward = Vector3.Cross(wallNormal,transform.up);
            controls.controller.Move(wallForward * wallRunSpeed * Time.deltaTime);

            if(Game.controls.Player.Jump.WasPressedThisFrame())
            {
                controls.velocity = (wallNormal + Vector3.up) * Mathf.Sqrt(jumpForce * -2 * Physics.gravity.y);
                SetWallRunActive(false);
            }
            if(!canWallRun)
            {
                SetWallRunActive(false);
                if(!controls.grounded) controls.velocity = wallForward * Mathf.Sqrt(wallRunSpeed * -2 * Physics.gravity.y);
            }
        }

        //Conditions for wall Run
        if(!canWallRun && Game.controls.Player.Jump.WasPerformedThisFrame())
        {
            SetWallRunActive(true);
        }

        
    }

    void SetWallRunActive(bool b)
    {
        if(b)
        {
            controls.moveEnabled = false;
            wallRunning = true;
        }
        else
        {
            controls.moveEnabled = true;
            wallRunning = false;
        }
    }
}
