using UnityEngine;

public abstract class MageState
{
    public abstract void Enter(Mage mage);

    public abstract void Update(Mage mage);

    public abstract void Collision(Mage mage);

}
