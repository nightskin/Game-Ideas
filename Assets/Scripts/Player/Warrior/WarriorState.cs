using UnityEngine;

public abstract class WarriorState
{
    public abstract void Enter(Warrior p);
    public abstract void Update(Warrior p);
    public abstract void Collision(Warrior p);
}
