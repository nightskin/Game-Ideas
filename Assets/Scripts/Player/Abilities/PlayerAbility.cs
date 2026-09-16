using UnityEngine;

[System.Serializable]
public class PlayerAbility
{
    public Player owner;

    public PlayerAbility(Player player)
    {
        owner = player;
    }

    public virtual void Init(){}
    public virtual void FixedUpdate(){}
    public virtual void Update(){}
}
