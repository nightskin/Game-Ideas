using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem : MonoBehaviour
{
    [SerializeField] FirstPersonPlayer player;
    [Range(0, 1)] [SerializeField] float wpnSensitivity;
    [SerializeField] Transform armPivot;
    [SerializeField] Animator animator;
    [SerializeField] Transform weapon;


    float atkAngle;
    bool isAttacking;
    Vector2 defSensitivity;
    Vector2 axis = new Vector2();

    void Start()
    {
        player.actions.Attack.performed += Attack_performed;
        player.actions.Attack.canceled += Attack_canceled;
        defSensitivity = player.sensitivity;
    }

    void Update()
    {
        if (isAttacking)
        {
            axis = player.actions.Look.ReadValue<Vector2>();
            if (axis.magnitude > 0)
            {
                atkAngle = Mathf.Atan2(axis.x, -axis.y) * Mathf.Rad2Deg;
                animator.SetInteger("r", 180);
            }
        }
    }


    private void Attack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isAttacking = true;
    }

    private void Attack_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isAttacking = false;
    }


    
    ///Animation Events
    public void StartAttack()
    {
        player.sensitivity *= wpnSensitivity;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        weapon.GetComponent<WeaponScript>().inUse = true;
    }
    
    public void EndAttack()
    {
        player.sensitivity = defSensitivity;
        animator.SetInteger("r", 0);
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        weapon.GetComponent<WeaponScript>().inUse = false;
    }

}
