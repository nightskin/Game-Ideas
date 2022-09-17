using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class Melee : MonoBehaviour
{
    [SerializeField] private FirstPersonPlayer player;
    
    public float wpnSpeed = 10;

    private Vector3 wpnPos;
    private float lookX;
    private float lookY;


    void Start()
    {

    }

    void Update()
    {
        lookX = player.actions.Look.ReadValue<Vector2>().x;
        lookY = player.actions.Look.ReadValue<Vector2>().y;
        wpnPos.x += lookX * wpnSpeed * Time.deltaTime;
        wpnPos.y -= lookY * wpnSpeed * Time.deltaTime;
        wpnPos.x = Mathf.Clamp(wpnPos.x, -45, 45);
        wpnPos.y = Mathf.Clamp(wpnPos.y, -25, 25);

        if (player.actions.Attack.IsPressed())
        {
            Attack();
        }
        else
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), 10 * Time.deltaTime);
        }


    }

    void Attack()
    {
        if (lookY != 0 || lookX != 0)
        {
            float rotZ = Mathf.Atan2(lookX, -lookY) * Mathf.Rad2Deg;
            transform.localEulerAngles = new Vector3(wpnPos.y, wpnPos.x, rotZ);
        }


    }


}
