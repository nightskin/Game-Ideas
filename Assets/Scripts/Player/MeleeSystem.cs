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
    public AxisType actionAxisType;
    private Vector2 defaultLookSpeed;
    private float defaultMoveSpeed;
    [SerializeField] [Range(0, 1)] private float actionDamp = 0.1f;

    private Vector2 actionAxis = new Vector2();
    public float atkAngle = 0;

    bool blocking = false;
    public float blockAngle = 0;

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
        if (actionAxisType == AxisType.MOVE) actionAxis = player.actions.Move.ReadValue<Vector2>();
        else if (actionAxisType == AxisType.LOOK) actionAxis = player.actions.Look.ReadValue<Vector2>();

        if (animator.GetBool("slash") && !blocking)
        {
            if (actionAxis.magnitude > 0)
            {
                atkAngle = Mathf.Atan2(actionAxis.x, -actionAxis.y) * (180 / Mathf.PI);
            }
        }
        if(blocking && !animator.GetBool("slash"))
        {
            if(actionAxis.magnitude > 0)
            {
                blockAngle = Mathf.Atan2(-actionAxis.x, actionAxis.y) * (180 / Mathf.PI) + 90;
                blockAngle = Mathf.Round(blockAngle / 45) * 45;
                blockAngle = Mathf.Clamp(blockAngle, 0, 180);
            }
            armPivot.localEulerAngles = Vector3.Lerp(armPivot.localEulerAngles, new Vector3(0, 0, blockAngle), 10 * Time.deltaTime);

        }
    }

    void OnDestroy()
    {
        player.actions.Slash.performed -= Slash_performed;
        player.actions.Slash.canceled -= Slash_canceled;
    }

    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetBool("slash", true);
        player.lookSpeed *= actionDamp;
    }

    private void Slash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetBool("slash", false);
    }

    private void Block_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        blocking = true;
        if (actionAxisType == AxisType.LOOK) player.lookSpeed *= actionDamp;
        if (actionAxisType == AxisType.MOVE) player.moveSpeed *= actionDamp;
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
        if (actionAxisType == AxisType.LOOK) player.lookSpeed *= actionDamp;
        if (actionAxisType == AxisType.MOVE) player.moveSpeed *= actionDamp;
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
