using UnityEngine;

public class PlayerHIT : PlayerState
{
    //Stun Variables For When The Player is Hit
    public Vector3 knockBack;
    public float stunTime = 0.2f;
    float stunTimer;

    public override void Enter(Player p)
    {
        stunTimer = stunTime;
    }

    public override void Update(Player p)
    {
        if(stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
        }
        else
        {
            p.SwitchState(p.alive);
        }
    }

    public override void Collision(Player p)
    {
        
    }
}
