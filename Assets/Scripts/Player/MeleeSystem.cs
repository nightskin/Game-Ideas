using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem : MonoBehaviour
{
    public Transform armPivot;
    public Transform weapon;

    private FirstPersonPlayer player;
    public Animator animator;


    public enum AxisType
    {
        LOOK,
        MOVE,
    }
    public AxisType atkAxisType;
    public AxisType blockAxisType;
    private Vector2 defaultLookSpeed;
    private float defaultMoveSpeed;
    [SerializeField] [Range(0, 1)] private float atkDamp = 0.1f;
    [SerializeField] [Range(0, 1)] private float blockDamp = 0.1f;
    [SerializeField] private float blockSpeed = 10;

    Vector2 atkAxis = new Vector2();
    Vector2 blockAxis = new Vector2();
    float atkAngle = 0;
    float blockAngle = 0;

    bool blocking = false;


    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
        defaultLookSpeed = player.lookSpeed;
        defaultMoveSpeed = player.moveSpeed;

        player.actions.Slash.performed += Slash_performed;
        player.actions.Slash.canceled += Slash_canceled;
        player.actions.Block.performed += Block_performed;
        player.actions.Block.canceled += Block_canceled;
    }

    void Update()
    {
        if (atkAxisType == AxisType.MOVE) atkAxis = player.actions.Move.ReadValue<Vector2>();
        else if (atkAxisType == AxisType.LOOK) atkAxis = player.actions.Look.ReadValue<Vector2>();

        if (blockAxisType == AxisType.MOVE) blockAxis = player.actions.Move.ReadValue<Vector2>();
        else if (blockAxisType == AxisType.LOOK) blockAxis = player.actions.Look.ReadValue<Vector2>();

        if (animator.GetBool("slash") && !blocking)
        {
            if (atkAxis.magnitude > 0.5f)
            {
                atkAngle = Mathf.Atan2(atkAxis.x, -atkAxis.y) * (180 / Mathf.PI);
            }
        }
        if(blocking && !animator.GetBool("slash"))
        {
            if(atkAxis.magnitude > 0.5f)
            {
                blockAngle = Mathf.Atan2(-blockAxis.x, blockAxis.y) * (180 / Mathf.PI) + 90;
                blockAngle = Mathf.Round(blockAngle / 45) * 45;
                blockAngle = Mathf.Clamp(blockAngle, 0, 180);
            }

            if (blockAngle == 0) armPivot.localRotation = Quaternion.Lerp(armPivot.localRotation, Quaternion.Euler(0, 0, 0), blockSpeed * Time.deltaTime);
            if (blockAngle == 45) armPivot.localRotation = Quaternion.Lerp(armPivot.localRotation, Quaternion.Euler(0, 0, 45), blockSpeed * Time.deltaTime);
            if (blockAngle == 90) armPivot.localRotation = Quaternion.Lerp(armPivot.localRotation, Quaternion.Euler(10, 0, 90), blockSpeed * Time.deltaTime);
            if (blockAngle == 135) armPivot.localRotation = Quaternion.Lerp(armPivot.localRotation, Quaternion.Euler(0, 0, 135), blockSpeed * Time.deltaTime);
            if (blockAngle == 180) armPivot.localRotation = Quaternion.Lerp(armPivot.localRotation, Quaternion.Euler(0, -78, 0), blockSpeed * Time.deltaTime);

        }



    }

    void OnDestroy()
    {
        player.actions.Slash.performed -= Slash_performed;
        player.actions.Slash.canceled -= Slash_canceled;
        player.actions.Block.performed -= Block_performed;
        player.actions.Block.canceled -= Block_canceled;
    }

    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetBool("slash", true);
        player.lookSpeed *= atkDamp;
    }

    private void Slash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetBool("slash", false);
    }

    private void Block_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        blocking = true;
        if (atkAxisType == AxisType.LOOK) player.lookSpeed *= blockDamp;
        if (atkAxisType == AxisType.MOVE) player.moveSpeed *= blockDamp;
    }

    private void Block_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        blocking = false;
        player.lookSpeed = defaultLookSpeed;
        player.moveSpeed = defaultMoveSpeed;
    }

    ///Animation Events
    public void StartAttack()
    {
        weapon.GetComponent<PlayerWeapon>().attacking = true;
        if (atkAxisType == AxisType.LOOK) player.lookSpeed *= atkDamp;
        if (atkAxisType == AxisType.MOVE) player.moveSpeed *= atkDamp;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
    }
    
    public void EndAttack()
    {
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        weapon.GetComponent<PlayerWeapon>().attacking = false;
        player.lookSpeed = defaultLookSpeed;
        player.moveSpeed = defaultMoveSpeed;
    }

}
