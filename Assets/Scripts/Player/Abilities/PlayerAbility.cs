using UnityEngine;

[System.Serializable]
public abstract class PlayerAbility
{
    public Player owner;
    public abstract void Init();
    public abstract void FixedUpdate();
    public abstract void Update();
}
