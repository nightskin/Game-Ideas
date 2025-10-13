using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatControls : MonoBehaviour
{
    public enum PlayerCombatState
    {
        IDLE,
        ATK,
        DEF,
    }
    [HideInInspector] public PlayerCombatState state = PlayerCombatState.IDLE;
    public Animator animator;
    public Transform armPivot;
    public PlayerMovement movement;
    public PlayerSword sword;
    [SerializeField][Range(0, 1)] float actionDamp = 0.1f;

    Vector2 actionVector;
    float atkAngle = 0;

    //Stun Variables For When The Player is Hit
    public bool stunned = false;
    [Min(0)] public float knockBackForce = 10;
    [SerializeField][Range(0, 1)] float stunTime = 1;
    float stunTimer = 0;

    //Ground Slam Variables
    bool slaming = false;
    float slamCoolDownTimer = 0;
    [SerializeField][Range(0, 1)] float slamCoolDown = 0.5f;
    [SerializeField] float slamForce = 100;

    void Start()
    {
        stunTimer = stunTime;
        Game.controls.Player.Attack.performed += Attack_performed;
        Game.controls.Player.Defend.performed += Defend_performed;
        Game.controls.Player.Defend.canceled += Defend_canceled;
        Game.controls.Player.Crouch.performed += Ground_Pound_performed;
    }

    void Update()
    {
        if (slaming)
        {
            if (movement.grounded)
            {
                slamCoolDownTimer -= Time.deltaTime;
                if(slamCoolDownTimer <= 0)
                {
                    slaming = false;
                }
            }
            else
            {
                movement.controller.Move(Vector3.down * slamForce * Time.deltaTime);
            }
        }
        else if(stunned)
        {
            
        }
        else
        {
            actionVector = Game.controls.Player.Look.ReadValue<Vector2>();
            if (Game.controls.Player.Attack.IsPressed())
            {
                atkAngle = Mathf.Atan2(actionVector.x, -actionVector.y) * 180 / Mathf.PI;
            }
            else if (Game.controls.Player.Defend.IsPressed())
            {
                animator.SetInteger("x", Mathf.RoundToInt(actionVector.x));
                animator.SetInteger("y", Mathf.RoundToInt(actionVector.y));
            }
        }
    }

    void OnDestroy()
    {
        Game.controls.Player.Attack.performed -= Attack_performed;
        Game.controls.Player.Defend.performed -= Defend_performed;
        Game.controls.Player.Defend.canceled -= Defend_canceled;
    }

    public void StunCountDown()
    {
        animator.SetTrigger("hit");
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0)
        {
            animator.ResetTrigger("hit");
            stunTimer = stunTime;
            stunned = false;
        }
    }

    private void Attack_performed(InputAction.CallbackContext obj)
    {
        if (Game.slowCameraMovementWhenAttacking) movement.lookSpeed *= actionDamp;

        Vector2 actionVector = Game.controls.Player.Look.ReadValue<Vector2>();
        animator.SetTrigger("slash");
    }

    private void Defend_performed(InputAction.CallbackContext obj)
    {
        if (Game.slowCameraMovementWhenDefending) movement.lookSpeed *= actionDamp;
    }

    private void Defend_canceled(InputAction.CallbackContext obj)
    {
        if (Game.slowCameraMovementWhenDefending) movement.lookSpeed = Game.mouseSensitivity;
    }

    private void Ground_Pound_performed(InputAction.CallbackContext context)
    {
        if (!movement.grounded)
        {
            movement.velocity = Vector3.zero;
            animator.SetTrigger("slash");
            atkAngle = 0;
            slamCoolDownTimer = slamCoolDown;
            slaming = true;
        }
    }

    //Animation Events
    [SerializeField] void StartSlash()
    {
        if (!sword.trail.gameObject.activeSelf) sword.trail.gameObject.SetActive(true);
        state = PlayerCombatState.ATK;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
    }

    [SerializeField] void EndSlash()
    {
        if (sword.trail.gameObject.activeSelf) sword.trail.gameObject.SetActive(false);
        for (int t = 0; t < sword.trail.positionCount; t++)
        {
            sword.trail.SetPosition(t, Vector3.zero);
        }
        state = PlayerCombatState.IDLE;
        armPivot.localEulerAngles = Vector3.zero;
        movement.lookSpeed = Game.mouseSensitivity;
    }

    [SerializeField] void StartBlock()
    {
        state = PlayerCombatState.DEF;
        armPivot.localEulerAngles = Vector3.zero;
    }
    
    [SerializeField] void EndBlock()
    {
        if(state != PlayerCombatState.IDLE) state = PlayerCombatState.IDLE;
    }
}
