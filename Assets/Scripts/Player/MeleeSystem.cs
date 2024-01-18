using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeleeSystem : MonoBehaviour
{
    [SerializeField] Transform armPivot;
    [SerializeField] Transform weapon;

    private FirstPersonPlayer player;
    public Animator animator;
    float atkAngle = 0;

    public bool lockedOn = false;
    public Transform lockOnTarget = null;

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
        player.actions.LockOn.performed += LockOn_performed;
        player.actions.LockOn.canceled += LockOn_canceled;
    }

    void Update()
    {
        if(animator.GetBool("slash"))
        {
            Vector2 axis = player.actions.Look.ReadValue<Vector2>();
            if(axis.magnitude > 0) atkAngle = Mathf.Atan2(axis.x, -axis.y) * 180 / Mathf.PI;
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

    private void LockOn_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        RaycastHit[] hits = Physics.RaycastAll(player.camera.position, player.camera.forward);
        if (hits.Length > 0)
        {
            for(int i = 0; i < hits.Length; i++) 
            {
                if (hits[i].transform.tag == "Enemy" || hits[i].transform.tag == "EnemyWeapon")
                {
                    lockOnTarget = hits[i].transform;
                }
            }
        }
        lockedOn = true;
    }

    private void LockOn_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        lockOnTarget = null;
        lockedOn = false;
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
