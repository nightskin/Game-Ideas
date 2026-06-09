using UnityEngine;

public class MageHIT : MageState
{
    public Vector3 knockBack;
    float t;
    public override void Enter(Mage mage)
    {
        mage.velocity = knockBack;
        t = 0;
    }
    public override void Update(Mage mage)
    {
        mage.Look();

        // When player hits the ground
        if (mage.grounded && mage.velocity.y < 0)
        {
            mage.velocity.y = 0;
        }
        
        //Gravity
        mage.velocity.y += -10 * Time.deltaTime;
        //Apply Forces
        mage.controller.Move(mage.velocity * Time.deltaTime);

        //Drag
        if(mage.velocity.x != 0 || mage.velocity.z != 0)
        {
            t += Time.deltaTime;
            mage.velocity.x = Mathf.Lerp(mage.velocity.x, 0, t);
            mage.velocity.z = Mathf.Lerp(mage.velocity.z, 0, t);
        }
        else
        {
            mage.SwtichState(mage.ok);
        }

    }

    public override void Collision(Mage mage)
    {
        mage.velocity = Vector3.zero;
    }

}
