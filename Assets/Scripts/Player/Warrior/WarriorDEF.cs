using UnityEngine;

public class WarriorDEF : WarriorState
{
    Vector2 blockValue;
    public override void Enter(Player player)
    {
        player.lookSpeed *= Game.slowCameraDefAmont;
        blockValue = Vector2.zero;
    }

    public override void Update(Player player)
    {
        player.Look();
        player.Move();

        blockValue += Game.controls.Player.Look.ReadValue<Vector2>();
        blockValue.x = Mathf.Clamp(blockValue.x, -1,1);
        blockValue.y = Mathf.Clamp01(blockValue.y);
        
        if(Game.controls.Player.Jump.WasPressedThisFrame() && player.grounded)
        {
            player.velocity = Vector3.up * Mathf.Sqrt(player.jumpHeight * -2 * Physics.gravity.y);
            player.jumping = true;
        }
        if(Game.controls.Player.Defend.WasReleasedThisFrame())
        {
            player.SwitchState(player.idle);
        }
        if(Game.controls.Player.Slash.IsPressed())
        {
            Vector2 atkVector = Game.controls.Player.Look.ReadValue<Vector2>();
            if(atkVector.magnitude > 0)
            {
                player.atkAngle = Mathf.Atan2(atkVector.x, -atkVector.y) * 180 / Mathf.PI;
                player.SwitchState(player.atk);
            }
        }
    }

    public override void Collision(Player player)
    {

    }
}
