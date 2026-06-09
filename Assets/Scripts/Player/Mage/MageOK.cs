using UnityEngine;

public class MageOK : MageState
{
    public override void Enter(Mage mage)
    {
        
    }
    public override void Update(Mage mage)
    {
        //For Debug only hurts player when pressed
        if(Game.controls.Player.Defend.IsPressed())
        {
            mage.hit.knockBack = -mage.camera.transform.forward * 100;
            mage.SwtichState(mage.hit);
        }

        mage.Look();
        mage.Move();
    }

    public override void Collision(Mage mage)
    {
        
    }

}
