using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem : MonoBehaviour
{
    [Range(0, 1)] public float wpnSensitivity;

    [SerializeField] Transform weapon;
    [SerializeField] Transform arm;

    float atkAngle;
    bool parry;
    bool isAttacking;
    Vector2 defSensitivity;
    FirstPersonPlayer player;
    public Animator animator;

    Vector2 look;
    RaycastHit hit;

    void Start()
    {
        player = GetComponent<FirstPersonPlayer>();
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
        if(!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            animator.SetInteger("r", 0);
        }


        if(isAttacking)
        {
            look = player.actions.Look.ReadValue<Vector2>();
            atkAngle = Mathf.Atan2(look.x, look.y) * Mathf.Rad2Deg;
            atkAngle = 45 * Mathf.Round(atkAngle / 45);
            animator.SetInteger("r", (int)atkAngle);
        }




    }

}
