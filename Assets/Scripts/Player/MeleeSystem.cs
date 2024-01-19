using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeleeSystem : MonoBehaviour
{
    [SerializeField] Transform armPivot;
    [SerializeField] Transform weapon;

    [SerializeField][Range(0, 1)] float atkDamp = 0.5f;
    
    FirstPersonPlayer player;
    public Animator animator;
    float atkAngle = 0;
    float defaultLookSpeed;

    
    public Transform lockOnTarget = null;

    public bool defending = false;
    public bool blocking = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
        defaultLookSpeed = player.lookSpeed;

        player.actions.Slash.performed += Slash_performed;
        player.actions.Slash.canceled += Slash_canceled;
        player.actions.Defend.performed += Defend_performed;
        player.actions.Defend.canceled += Defend_canceled;
        player.actions.LockOn.performed += LockOn_performed;
        player.actions.LockOn.canceled += LockOn_canceled;
    }

    void Update()
    {
        if(animator.GetBool("slash"))
        {
            Vector2 axis = player.actions.Look.ReadValue<Vector2>();
            if(axis.magnitude > 0.5f) atkAngle = Mathf.Atan2(axis.x, -axis.y) * 180 / Mathf.PI;
        }
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
        player.actions.LockOn.performed -= LockOn_performed;
        player.actions.LockOn.canceled -= LockOn_canceled;
    }

    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetBool("slash", true);
        player.lookSpeed *= atkDamp;
    }

    private void Slash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetBool("slash", false);
        player.lookSpeed = defaultLookSpeed;
    }

    private void Defend_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        defending = true;
    }

    private void Defend_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        defending = false;
    }

    private void LockOn_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(Physics.Raycast(player.camera.position, player.camera.forward, out RaycastHit hit))
        {
            if(hit.transform.gameObject.layer == 6)
            {
                lockOnTarget = hit.transform;
            }
        }
    }

    private void LockOn_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        lockOnTarget = null;
    }



    ///Animation Events
    public void StartAttack()
    {
        weapon.GetComponent<PlayerWeapon>().attacking = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        player.lookSpeed *= atkDamp;
    }
    
    public void EndAttack()
    {
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        weapon.GetComponent<PlayerWeapon>().attacking = false;
        player.lookSpeed = defaultLookSpeed;
    }

}
