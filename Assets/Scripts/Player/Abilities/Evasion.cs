using UnityEngine;


public class Evasion : PlayerAbility
{
    public bool active = false;
    public float dashSpeed = 150;
    float maxKeyboardPressTime = 0.25f; 
    float keyboardPressTime = 0;
    float evadeTimer = 0;
    float maxEvadeTime = 0.1f;

    Vector3 dashDirection;
    Vector2 evadeInput = Vector2.zero;
    Vector2 prevMoveInput = Vector2.zero;


    public override void Init()
    {
        
    }

    public override void FixedUpdate()
    {
        
    }

    public override void Update()
    {
        Vector2 moveInput = Game.input.Player.Move.ReadValue<Vector2>();

        if(EvadeKeyboardInput() && !active)
        {
            evadeTimer = 0;
            active = true;
            evadeInput = prevMoveInput;
            dashDirection = (owner.transform.right * evadeInput.x + owner.transform.forward * evadeInput.y).normalized;
        }
        else if(Game.input.Player.EvadeG.WasPerformedThisFrame() && !active)
        {
            evadeTimer = 0;
            active = true;
            evadeInput = moveInput;
            dashDirection = (owner.transform.right * evadeInput.x + owner.transform.forward * evadeInput.y).normalized;
        }

        if(active)
        {
            if(evadeTimer < maxEvadeTime)
            {
                owner.controller.Move(dashDirection * owner.currentSpeed * Time.deltaTime);
                evadeTimer += Time.deltaTime;
            }
            else
            {
                active = false;
            }
        }

        prevMoveInput = moveInput;
    }
    

    // built-in tapping checks for some reason does not work so I had to implement my own
    bool EvadeKeyboardInput()
    {
        if(Game.input.Player.EvadeK.WasPerformedThisFrame())
        {
            return false;
        }
        else if(Game.input.Player.EvadeK.IsPressed())
        {
            keyboardPressTime += Time.deltaTime;
            return false;
        }
        else if(Game.input.Player.EvadeK.WasReleasedThisFrame())
        {
            if(keyboardPressTime < maxKeyboardPressTime)
            {
                keyboardPressTime = 0;
                return true;
            }
            else
            {
                keyboardPressTime = 0;
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
