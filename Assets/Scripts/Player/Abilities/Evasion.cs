using UnityEngine;

[System.Serializable]
public class Evasion : PlayerAbility
{
    bool isEvading = false;
    float evadeSpeed = 100;
    float maxKeyboardPressTime = 0.2f; 
    float keyboardPressTime = 0;
    float evadeTimer = 0;
    float maxEvadeTime = 0.1f;

    Vector3 dashDirection;
    Vector2 evadeInput = Vector2.zero;
    Vector2 prevMoveInput = Vector2.zero;

    public Evasion(Player player) : base(player)
    {
        
    }

    public override void Init()
    {
        
    }

    public override void FixedUpdate()
    {
        if(isEvading)
        {
            owner.targetSpeed = evadeSpeed;
        }
        else
        {
            owner.targetSpeed = owner.normalSpeed;
        }
    }

    public override void Update()
    {
        if(owner.onGround)
        {
            Vector2 moveInput = Game.input.Player.Move.ReadValue<Vector2>();

            if(EvadeKeyboardInput() && !isEvading)
            {
                isEvading = true;
                evadeInput = prevMoveInput;
                dashDirection = (owner.transform.right * evadeInput.x + owner.transform.forward * evadeInput.y).normalized;
            }
            else if(Game.input.Player.EvadeG.WasPerformedThisFrame() && !isEvading)
            {
                isEvading = true;
                evadeInput = moveInput;
                dashDirection = (owner.transform.right * evadeInput.x + owner.transform.forward * evadeInput.y).normalized;
            }

            if(isEvading)
            {
                if(evadeTimer < maxEvadeTime)
                {
                    owner.canMove = false;
                    owner.controller.Move(dashDirection * owner.currentSpeed * Time.deltaTime);
                    evadeTimer += Time.deltaTime;
                }
                else
                {
                    owner.canMove = true;
                    isEvading = false;
                    evadeTimer = 0;
                }
            }

            prevMoveInput = moveInput;
        }
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
