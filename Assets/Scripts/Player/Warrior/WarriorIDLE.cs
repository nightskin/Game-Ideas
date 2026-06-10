using UnityEngine;

public class WarriorIDLE : WarriorState
{
    public override void Enter(Warrior player)
    {
        player.animator.SetFloat("defx", 0);
        player.animator.SetFloat("defy", 0);
        player.lookSpeed = Game.aimSense;
    }

    public override void Update(Warrior player)
    {
        player.Look();
        player.Move();

        if (Game.controls.Player.PimaryAction.IsPressed())
        {
            player.SwitchState(player.atk);
        }
        else if(Game.controls.Player.SecondaryAction.IsPressed())
        {
            player.SwitchState(player.def);
        }
    }

    public override void Collision(Warrior p)
    {
        
    }
}
