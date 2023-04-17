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
        MOVE
    }
    public AxisType atkAxisType;
    private Vector2 defaultLookSpeed;
    private float defaultMoveSpeed;
    [SerializeField] [Range(0,1)] private float atkDamp = 0.1f;

    private Vector2 axis = new Vector2();
    [SerializeField] private float atkAngle;


    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
        defaultLookSpeed = player.lookSpeed;
        defaultMoveSpeed = player.moveSpeed;

        player.actions.Slash.performed += Slash_performed;
    }

    void Update()
    {
        if(animator.GetBool("slash"))
        {
            if (atkAxisType == AxisType.MOVE) axis = player.actions.Move.ReadValue<Vector2>();
            else if (atkAxisType == AxisType.LOOK) axis = player.actions.Look.ReadValue<Vector2>();

            if (axis.magnitude > 0)
            {
                atkAngle = Mathf.Atan2(axis.x, -axis.y) * (180 / Mathf.PI);
            }
        }



    }

    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetBool("slash", true);
        player.lookSpeed *= atkDamp;

    }

    ///Animation Events
    public void StartAttack()
    {
        weapon.GetComponent<PlayerWeapon>().attacking = true;
        if (atkAxisType == AxisType.LOOK) player.lookSpeed *= atkDamp;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
    }
    
    public void EndAttack()
    {
        player.lookSpeed = defaultLookSpeed;
        armPivot.localEulerAngles = Vector3.zero;
        weapon.GetComponent<PlayerWeapon>().attacking = false;
        animator.SetBool("slash", false);
    }

}
