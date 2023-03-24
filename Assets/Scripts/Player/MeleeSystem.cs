using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem : MonoBehaviour
{
    public Transform armPivot;
    public Transform weapon;

    private FirstPersonPlayer player;
    private Animator animator;

    private bool attacking;

    public enum AxisType
    {
        LOOK,
        MOVE
    }
    public AxisType atkAxisType;
    private Vector2 defaultLookSpeed;
    [SerializeField] [Range(0,1)] private float LookDamp = 0.1f;

    private Vector2 axis = new Vector2();
    [SerializeField] private float atkAngle;


    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
        defaultLookSpeed = player.lookSpeed;

        player.actions.Slash.performed += Slash_performed;
        player.actions.Slash.canceled += Slash_canceled;
    }

    void Update()
    {
        if (atkAxisType == AxisType.MOVE) axis = player.actions.Move.ReadValue<Vector2>();
        else if (atkAxisType == AxisType.LOOK) axis = player.actions.Look.ReadValue<Vector2>();

        if (attacking)
        {
            if (axis.magnitude > 0)
            {
                atkAngle = Mathf.Atan2(axis.x, -axis.y) * (180 / Mathf.PI);
            }
            else
            {
                atkAngle = Random.Range(-180, 180);
            }
            animator.SetBool("slash", true);
        }

    }


    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        attacking = true;
        player.lookSpeed *= LookDamp;
    }

    private void Slash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        attacking = false;
        player.lookSpeed = defaultLookSpeed;
    }



    ///Animation Events
    public void StartAttack()
    {
        weapon.GetComponent<WeaponScript>().attacking = true;
        if (atkAxisType == AxisType.LOOK) player.lookSpeed *= 0.1f;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
    }
    
    public void EndAttack()
    {
        player.lookSpeed = defaultLookSpeed;
        armPivot.localEulerAngles = Vector3.zero;
        weapon.GetComponent<WeaponScript>().attacking = false;
        animator.SetBool("slash", false);
    }

}
