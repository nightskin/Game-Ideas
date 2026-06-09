using UnityEngine;

public class WarriorHIT : WarriorState
{
    //Stun Variables For When The Player is Hit
    public Vector3 knockBack;
    public float stunTime = 0.2f;
    float stunTimer;

    public override void Enter(Player player)
    {
        stunTimer = stunTime;
    }

    public override void Update(Player player)
    {
        if(stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
        }
        else
        {
            player.SwitchState(player.idle);
        }
    }

    public override void Collision(Player player)
    {
        
    }
}
