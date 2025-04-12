using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombatControls : MonoBehaviour
{
    public Transform armPivot;
    public PlayerMeleeWeapon sword;


    [SerializeField][Range(0, 1)] float actionDamp = 0.1f;
    float defaultLookSpeed;

    public Vector2 atkVector = Vector2.zero;
    public float atkAngle = 180;

    public PlayerMovement player;
    public Animator animator;

    void Start()
    {
        defaultLookSpeed = player.lookSpeed;
        if (sword.trail) 
        { 
            sword.trail.gameObject.SetActive(false);
        }
        animator = GetComponent<Animator>();
        player = GetComponent<PlayerMovement>();

        player.actions.Attack.performed += Slash_performed;
        player.actions.Attack.canceled += Slash_canceled;

    }
    
    void Update()
    {
        atkVector = player.actions.Look.ReadValue<Vector2>().normalized;
        if (player.actions.Attack.IsPressed())
        {
            sword.ChargeWeapon();
            if (atkVector.magnitude > 0)
            {
                atkAngle = Mathf.Atan2(atkVector.x, -atkVector.y) * 180 / Mathf.PI;
            }
        }

    }

    void OnDestroy()
    {
        player.actions.Attack.performed -= Slash_performed;
        player.actions.Attack.canceled -= Slash_canceled;
    }
    
    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetTrigger("slash");
    }

    private void Slash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(sword.chargeValue >= sword.maxChargeValue) 
        {
            animator.SetTrigger("slash");
        }
        else
        {
            sword.ReleaseCharge();
        }
    }
    
    

    ///Animation Events
    public void BlockCharge()
    {
        sword.canCharge = false;
    }

    public void AllowCharge()
    {
        sword.canCharge = true;
    }

    public void StartSlash()
    {
        sword.slashing = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        if(sword.trail)
        {
            sword.trail.gameObject.SetActive(true);
        }
        player.lookSpeed *= actionDamp;
    }
    
    public void EndSlash()
    {
        sword.slashing = false;
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        if (sword.trail)
        {
            sword.trail.gameObject.SetActive(false);
        }
        player.lookSpeed = defaultLookSpeed;
    }
    
    public void ChargeSlash()
    {
        sword.ReleaseCharge();
    }
}
