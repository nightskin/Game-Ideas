using UnityEngine;

public abstract class PlayerState
{
    public abstract void Enter(Player p);
    public abstract void Update(Player p);
    public abstract void Collision(Player p);
}
