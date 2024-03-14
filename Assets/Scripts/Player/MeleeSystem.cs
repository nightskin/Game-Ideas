using UnityEngine;

public class MeleeSystem : MonoBehaviour
{
    [SerializeField] Transform armPivot;
    [SerializeField] Transform weapon;

    [SerializeField][Range(0, 1)] float actionDamp = 0.5f;
    
    FirstPersonPlayer player;
    public Animator animator;


    float atkAngle = 0;
    public Vector3 atkVector;
    


    [SerializeField] float blockSpeed = 10;
    bool defending = false;
    float blockAngle = 0;
    Vector3 TargetArmPivotAngle = Vector3.zero;

    float defaultLookSpeed;
    public Transform lockOnTarget = null;


    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
        defaultLookSpeed = player.lookSpeed;

        player.actions.Slash.performed += Slash_performed;
        player.actions.Slash.canceled += Slash_canceled;
        player.actions.Defend.performed += Defend_performed;
        player.actions.Defend.canceled += Defend_canceled;
        player.actions.LockOn.performed += LockOn_performed;
        player.actions.LockOn.canceled += LockOn_canceled;
    }

    void Update()
    {
        if(animator.GetBool("slash"))
        {
            Vector2 axis = player.actions.Look.ReadValue<Vector2>();
            if (axis.magnitude > 0.5f)
            {
                atkAngle = Mathf.Atan2(axis.x, -axis.y) * 180 / Mathf.PI;
                atkVector = axis;
            }

        }
        else if (defending && !animator.GetBool("slash")) 
        {
            Vector2 axis = player.actions.Look.ReadValue<Vector2>();
            if (axis.magnitude > 0.5f) 
            { 
                blockAngle = Mathf.Atan2(axis.x, -axis.y) * 180 / Mathf.PI;
                blockAngle = Mathf.Round(blockAngle / 45) * 45;
                if(blockAngle == -180 || blockAngle == 180 || blockAngle == 135 || blockAngle == -135)
                {
                    TargetArmPivotAngle = new Vector3(10, 0, 90);
                }
                else if(blockAngle == 90)
                {
                    TargetArmPivotAngle = new Vector3(0, 10, 0);
                }
                else if(blockAngle == -90)
                {
                    TargetArmPivotAngle = new Vector3(0, -90, 0);
                }
            }
            armPivot.localRotation = Quaternion.Lerp(armPivot.localRotation, Quaternion.Euler(TargetArmPivotAngle), blockSpeed * Time.deltaTime);
        }
    }

    void OnDestroy()
    {
        player.actions.Slash.performed -= Slash_performed;
        player.actions.Slash.canceled -= Slash_canceled;
        player.actions.Defend.performed -= Defend_performed;
        player.actions.Defend.canceled -= Defend_canceled;
        player.actions.LockOn.performed -= LockOn_performed;
        player.actions.LockOn.canceled -= LockOn_canceled;
    }

    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetBool("slash", true);
        player.lookSpeed *= actionDamp;
    }

    private void Slash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        player.lookSpeed = defaultLookSpeed;
    }

    private void Defend_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        defending = true;
        player.lookSpeed *= actionDamp;
    }

    private void Defend_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        defending = false;
        player.lookSpeed = defaultLookSpeed;
    }

    private void LockOn_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(Physics.Raycast(player.camera.position, player.camera.forward, out RaycastHit hit))
        {
            if(hit.transform.gameObject.layer == 6)
            {
                lockOnTarget = hit.transform;
            }
        }
    }

    private void LockOn_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        lockOnTarget = null;
    }



    ///Animation Events
    public void StartAttack()
    {
        TargetArmPivotAngle = Vector3.zero;
        armPivot.localRotation = Quaternion.Euler(0, 0, 0);
        weapon.GetComponent<PlayerWeapon>().attacking = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        player.lookSpeed *= actionDamp;
    }
    
    public void EndAttack()
    {
        animator.SetBool("slash", false);
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        weapon.GetComponent<PlayerWeapon>().attacking = false;
        player.lookSpeed = defaultLookSpeed;
    }

}
