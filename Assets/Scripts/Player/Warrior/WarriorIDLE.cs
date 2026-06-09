using UnityEngine;

public class WarriorIDLE : WarriorState
{
    public override void Enter(Player player)
    {
        player.lookSpeed = Game.aimSense;
    }

    public override void Update(Player player)
    {
        player.Look();
        player.Move();

        if (Game.controls.Player.Slash.IsPressed())
        {
            Vector2 atkVector = Game.controls.Player.Look.ReadValue<Vector2>();
            if(atkVector.magnitude > 0)
            {
                player.atkAngle = Mathf.Atan2(atkVector.x, -atkVector.y) * 180 / Mathf.PI;
                player.SwitchState(player.atk);
            }
        }
        else if(Game.controls.Player.Defend.IsPressed())
        {
            player.SwitchState(player.def);
        }
    }

    public override void Collision(Player p)
    {
        
    }
}
