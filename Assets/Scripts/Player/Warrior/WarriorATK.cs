using UnityEngine;

public class WarriorAtk : WarriorState
{
    public float angle;
    public override void Enter(Warrior player)
    {
        player.animator.SetFloat("defx", 0);
        player.animator.SetFloat("defy", 0);

        player.lookSpeed *= Game.slowCameraAtkAmount;
        Vector2 atkVector = Game.controls.Player.Look.ReadValue<Vector2>();
        angle = Mathf.Atan2(atkVector.x, -atkVector.y) * 180 / Mathf.PI;
        player.animator.SetTrigger("atk");
    }

    public override void Update(Warrior player)
    {
        player.Look();
        player.Move();

        Vector2 atkVector = Game.controls.Player.Look.ReadValue<Vector2>();
        angle = Mathf.Atan2(atkVector.x, -atkVector.y) * 180 / Mathf.PI;

        if(Game.controls.Player.PimaryAction.WasPerformedThisFrame())
        {
            player.animator.SetTrigger("atk");
        }

    }

    public override void Collision(Warrior player)
    {
        
    }    
}
