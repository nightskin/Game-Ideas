using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem2 : MonoBehaviour
{
    [Range(0, 1)] public float wpnSensitivity; 
    public Transform weapon;

    float rot;
    bool parry;
    bool atk;
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
    }

    private void Attack_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        look = player.actions.Look.ReadValue<Vector2>();
        rot = Mathf.Atan2(look.x, look.y) * Mathf.Rad2Deg;
        rot = 45 * Mathf.Round(rot / 45);
        animator.SetInteger("r", (int)rot);

        player.sensitivity = defSensitivity;
    }

    void Update()
    {
        //prevents slashing animations from repeating
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            animator.SetInteger("r", 0);
        }

    }

}
