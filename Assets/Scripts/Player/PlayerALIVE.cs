using UnityEngine;

public class PlayerALIVE : PlayerState
{
    public override void Enter(Player p)
    {
        
    }

    public override void Update(Player p)
    {
        p.Move();
        p.Look();
    }

    public override void Collision(Player p)
    {
        
    }
}
