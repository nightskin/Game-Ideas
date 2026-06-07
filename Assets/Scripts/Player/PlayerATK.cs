using UnityEngine;

public class PlayerAtk : PlayerState
{
    public override void Enter(Player player)
    {
        player.lookSpeed *= Game.slowCameraAtkAmount;
        player.animator.SetTrigger("slash");
    }

    public override void Update(Player player)
    {
        player.Look();
        player.Move();
    }

    public override void Collision(Player player)
    {
        
    }    
}
