using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 100;
    [SerializeField] int health;
    MeleeSystem meleeSystem;
    FirstPersonPlayer player;
    [SerializeField] PlayerHUD hud;


    void Start()
    {
        if(!player) player = GetComponent<FirstPersonPlayer>();
        if(!meleeSystem) meleeSystem = GetComponent<MeleeSystem>();
        if(!hud) hud = GameObject.Find("HUD").GetComponent<PlayerHUD>();
        health = maxHealth;
    }

    void Update()
    {
        
    }

    public void Heal(int amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
    }

    public void TakeDamage()
    {
        health--;
        hud.Flicker();
        meleeSystem.animator.SetTrigger("recoil");
        if (health == 0)
        {
            hud.FadeToBlack();
        }
    }
    
}
