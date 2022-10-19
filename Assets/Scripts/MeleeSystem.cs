using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem : MonoBehaviour
{
    [Range(0, 1)] public float wpnSensitivity;
    public enum meleeSystemType
    { 
        CONTINUOUS,
        NON_CONTINOUS
    }
    public meleeSystemType meleeSystem;

    [SerializeField] Transform weapon;
    [SerializeField] Transform arm;

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
        player.actions.Parry.performed += Parry_performed;
        defSensitivity = player.sensitivity;
    }

    private void Parry_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    private void Attack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        atk = true;
        player.sensitivity *= wpnSensitivity;
    }

    private void Attack_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(meleeSystem == meleeSystemType.NON_CONTINOUS)
        {

            look = player.actions.Look.ReadValue<Vector2>();
            rot = Mathf.Atan2(look.x, look.y) * Mathf.Rad2Deg;
            rot = 45 * Mathf.Round(rot / 45);
            animator.SetInteger("r", (int)rot);
        }

        atk = false;
        player.sensitivity = defSensitivity;
    }

    void Update()
    {
        if(meleeSystem == meleeSystemType.NON_CONTINOUS)
        {
            //prevents slashing animations from repeating
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                animator.SetInteger("r", 0);
            }
        }
        if(meleeSystem == meleeSystemType.CONTINUOUS)
        {
            if(atk)
            {
                look = player.actions.Look.ReadValue<Vector2>();
                rot = Mathf.Atan2(look.x, look.y) * Mathf.Rad2Deg;
                rot = 45 * Mathf.Round(rot / 45);
                animator.SetInteger("r", (int)rot);
            }
        }



    }

}
