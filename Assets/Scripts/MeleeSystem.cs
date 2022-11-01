using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem : MonoBehaviour
{
    [SerializeField] FirstPersonPlayer player;
    [Range(0, 1)] [SerializeField] float wpnSensitivity;
    [SerializeField] Transform armPivot;
    [SerializeField] float cooldown = 1;

    float cooldownTimer = 0;
    public float atkAngle;
    Animator animator;
    bool isAttacking;
    Vector2 defSensitivity;
    Vector2 axis = new Vector2();

    void Start()
    {
        animator = GetComponent<Animator>();
        player.actions.Attack.performed += Attack_performed;
        player.actions.Attack.canceled += Attack_canceled;
        defSensitivity = player.sensitivity;
    }


    private void Attack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        player.sensitivity *= wpnSensitivity;
        isAttacking = true;
    }

    private void Attack_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        player.sensitivity = defSensitivity;
        isAttacking = false;
    }

    void Update()
    {
        if(isAttacking)
        {
            axis = player.actions.Look.ReadValue<Vector2>();
            if(axis.magnitude > 0)
            {
                atkAngle = Mathf.Atan2(axis.x, -axis.y) * Mathf.Rad2Deg;
                animator.SetInteger("r", 180);
            }
        }
    }

    public void SetAttackAngle()
    {
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
    }
    public void ResetArm()
    {
        animator.SetInteger("r", 0);
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
    }

}
