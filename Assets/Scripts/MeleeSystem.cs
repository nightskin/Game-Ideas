using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem : MonoBehaviour
{
    [Range(0, 1)] public float wpnSensitivity;
    [SerializeField] Transform armPivot;
    [SerializeField] float atkAngle;
    
    bool isAttacking;
    Vector2 defSensitivity;
    FirstPersonPlayer player;
    public Animator animator;

    Vector2 axis = new Vector2();
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


        if (isAttacking)
        {
            axis = player.actions.Look.ReadValue<Vector2>();
            if(axis.x  != 0 && axis.y != 0) atkAngle = Mathf.Atan2(axis.x, -axis.y) * Mathf.Rad2Deg;
            armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
            animator.SetInteger("r", 180);
        }
        else
        {
            animator.SetInteger("r", 0);
            armPivot.localEulerAngles = new Vector3(0, 0, 0);
        }





    }

}
