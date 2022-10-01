using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem2 : MonoBehaviour
{
    public float thresholdX = 0.5f;
    public float thresholdY = 0.5f;

    FirstPersonPlayer player;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
    }

    
    void Update()
    {
        Vector2 look = player.actions.Look.ReadValue<Vector2>();
        if(player.actions.Attack.IsPressed())
        {
            float rot = Mathf.Atan2(look.x, look.y) * Mathf.Rad2Deg;
            rot = 45 * Mathf.Round(rot / 45);
            Debug.Log(rot);
            animator.SetInteger("r", (int)rot);
        }
    }
}
