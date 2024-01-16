using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeleeSystem : MonoBehaviour
{
    [SerializeField] Transform armPivot;
    [SerializeField] Transform weapon;
    [SerializeField] Image crossHair;

    private FirstPersonPlayer player;
    public Animator animator;
    float atkAngle = 0;

    public bool defending = false;
    public bool blocking = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();


        player.actions.Slash.performed += Slash_performed;
        player.actions.Slash.canceled += Slash_canceled;
        player.actions.Defend.performed += Defend_performed;
        player.actions.Defend.canceled += Defend_canceled;
        player.actions.RotateAngle.performed += RotateAngle_performed;
    }

    void Update()
    {
        if (defending) 
        {
            if (player.moveDirection != Vector3.zero)
            {
                player.dashing = true;
            }
            else
            {
                blocking = true;
            }

        }
    }


    void OnDestroy()
    {
        player.actions.Slash.performed -= Slash_performed;
        player.actions.Slash.canceled -= Slash_canceled;
        player.actions.Defend.performed -= Defend_performed;
        player.actions.Defend.canceled -= Defend_canceled;
    }

    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetBool("slash", true);
    }

    private void Slash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetBool("slash", false);
    }

    private void Defend_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        defending = true;
    }

    private void Defend_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        defending = false;
    }

    private void RotateAngle_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        float rotateValue = obj.ReadValue<float>();
        if(rotateValue == 1)
        {
            atkAngle += 45;
        }
        else if(rotateValue == -1)
        {
            atkAngle -= 45;
        }
        crossHair.rectTransform.rotation = Quaternion.Euler(0, 0, atkAngle);
    }

    ///Animation Events
    public void StartAttack()
    {
        weapon.GetComponent<PlayerWeapon>().attacking = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
    }
    
    public void EndAttack()
    {
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        weapon.GetComponent<PlayerWeapon>().attacking = false;
    }

}
