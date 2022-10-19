using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem2 : MonoBehaviour
{
    [Range(0, 1)] public float wpnSensitivity; 
    public Transform weapon;

    bool parry;
    bool atk;
    float defSensitivity;
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
        atk = true;
        player.sensitivity *= wpnSensitivity;
    }

    private void Attack_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        atk = false;
        player.sensitivity = defSensitivity;
    }

    void Update()
    {
        look = player.actions.Look.ReadValue<Vector2>();

        if(atk)
        {
            float rot = Mathf.Atan2(look.x, look.y) * Mathf.Rad2Deg;
            rot = 45 * Mathf.Round(rot / 45);
            animator.SetInteger("r", (int)rot);
            if(Physics.BoxCast(weapon.position, weapon.GetComponent<BoxCollider>().size, weapon.forward, out hit))
            {
                if (hit.transform.tag == "Moveable")
                {
                    hit.rigidbody.AddExplosionForce(10, hit.point, 0.5f);
                }
            }
        }



    }

}
