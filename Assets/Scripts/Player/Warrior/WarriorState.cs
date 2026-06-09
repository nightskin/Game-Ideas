using UnityEngine;

public abstract class WarriorState
{
    public abstract void Enter(Player p);
    public abstract void Update(Player p);
    public abstract void Collision(Player p);
}
