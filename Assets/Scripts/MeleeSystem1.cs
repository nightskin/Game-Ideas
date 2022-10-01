using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class MeleeSystem1 : MonoBehaviour
{
    [SerializeField] private FirstPersonPlayer player;
    [SerializeField] private Transform pivot;
    [SerializeField] private Transform hand;

    public float wpnSpeed = 3000;

    private Vector3 wpnPos;
    private float lookX;
    private float lookY;

    private float rotX;
    private float rotZ;
    private float rotY;

    void Start()
    {

    }

    void Update()
    {
        lookX = player.actions.Look.ReadValue<Vector2>().x;
        lookY = player.actions.Look.ReadValue<Vector2>().y;
        //wpnPos.y = Mathf.Clamp(wpnPos.y, -25, 25);

        if (player.actions.Attack.IsPressed())
        {
            Attack();
        }
        else
        {
            
        }


    }

    void Attack()
    {
        if (lookY != 0 || lookX != 0)
        {
            rotZ = Mathf.Atan2(lookX, -lookY) * Mathf.Rad2Deg;
            rotX = lookX * 90;
            //rotY = lookX * 25;
            rotX = Mathf.Clamp(rotX, -45, 45);
        }
        pivot.localRotation = Quaternion.Lerp(pivot.localRotation, Quaternion.Euler(0, rotX, rotZ), wpnSpeed * Time.deltaTime);
    }

    void Parry()
    {

    }

}
