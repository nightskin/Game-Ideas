using UnityEngine;

public class WarriorDEF : WarriorState
{
    Vector2 blockValue;
    public override void Enter(Warrior player)
    {
        player.lookSpeed *= Game.slowCameraDefAmont;
        blockValue = Vector2.zero;
        player.animator.SetBool("def", true);
    }

    public override void Update(Warrior player)
    {
        player.Look();
        player.Move();

        blockValue += Game.controls.Player.Look.ReadValue<Vector2>() *  10 * Time.deltaTime;
        blockValue.x = Mathf.Clamp(blockValue.x, -1,1);
        blockValue.y = Mathf.Clamp01(blockValue.y);
        player.animator.SetFloat("defx", blockValue.x);
        player.animator.SetFloat("defy", blockValue.y);
        
        if(Game.controls.Player.SecondaryAction.WasReleasedThisFrame())
        {
            player.SwitchState(player.idle);
        }
        if(Game.controls.Player.PimaryAction.WasPerformedThisFrame())
        {
            player.SwitchState(player.atk);
        }
    }

    public override void Collision(Warrior player)
    {

    }
}
