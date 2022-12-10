using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem : MonoBehaviour
{
    [SerializeField] private FirstPersonPlayer player;
    [SerializeField] private Animator animator;

    
    [SerializeField] Transform armPivot;
    [SerializeField] Transform weapon;


    float atkAngle;
    private float wpnSensitivity = 0.1f;
    private bool atk;
    private bool parry;

    Vector2 defSensitivity;
    Vector2 lookAxis = new Vector2();
    Vector2 moveAxis = new Vector2();

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
        defSensitivity = player.lookSpeed;
        player.actions.Slash.performed += Slash_performed;
        player.actions.Slash.canceled += Slash_canceled;
        player.actions.Parry.performed += Parry_performed;
        player.actions.Parry.canceled += Parry_canceled;
    }

    private void Parry_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        parry = false;
    }

    private void Parry_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        parry = true;
    }

    private void Slash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        atk = false;   
    }

    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        atk = true;
    }

    void Update()
    {
        lookAxis = player.actions.Look.ReadValue<Vector2>();
        moveAxis = player.actions.Move.ReadValue<Vector2>();
        if(atk)
        {
            if (lookAxis.magnitude > 0)
            {
                atkAngle = Mathf.Atan2(lookAxis.x, -lookAxis.y) * (180 / Mathf.PI);
            }
            animator.SetTrigger("atk");
        }
        else if(parry)
        {
            
        }
    }

    
    ///Animation Events
    public void StartAttack()
    {
        player.lookSpeed *= wpnSensitivity;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
    }
    
    public void EndAttack()
    {
        player.lookSpeed = defSensitivity;
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
    }

}
