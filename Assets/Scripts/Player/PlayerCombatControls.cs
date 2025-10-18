using System;
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
    [SerializeField] GameObject slashProjectile;
    public Animator animator;
    public Transform armPivot;
    public PlayerMovement movement;
    public PlayerSword sword;
    [SerializeField][Range(0, 1)] float lookDamp = 0.1f;
    Vector2 actionVector;
    float atkAngle = 0;

    // charging Variables
    float chargeDelayTimer = 0;
    [SerializeField] float chargeDelay = 0.25f; 

    //Stun Variables For When The Player is Hit
    [HideInInspector] public bool stunned = false;
    [HideInInspector] public Vector3 knockBackForce;
    [Range(0, 1)] float stunTime = 0.05f;
    float stunTimer = 0;

    //Ground Slam Variables
    [SerializeField] GameObject shockWave;
    bool slaming = false;
    float slamTimer = 0;
    [SerializeField] float slamCoolDown = 0.5f; 
    [SerializeField] float slamForce = 100;

    void Start()
    {
        stunTimer = stunTime;
        Game.controls.Player.Attack.performed += Attack_performed;
        Game.controls.Player.Attack.canceled += Attack_canceled;
        Game.controls.Player.Defend.performed += Defend_performed;
        Game.controls.Player.Defend.canceled += Defend_canceled;
        Game.controls.Player.Crouch.performed += GroundPound_performed;
    }

    void Update()
    {
        if (slaming)
        {
            if (movement.grounded)
            {
                if (shockWave)
                {
                    shockWave.gameObject.SetActive(true);
                }
                if(slamTimer < slamCoolDown)
                {
                    slamTimer += Time.deltaTime;
                }
                else
                {
                    movement.isCrouching = false;
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
                if (sword.IsSwordMagical())
                {
                    chargeDelayTimer -= Time.deltaTime;
                    if (chargeDelayTimer <= 0)
                    {
                        sword.ChargeWeapon();
                    }
                }
            }
            //else if (Game.controls.Player.Defend.IsPressed())
            //{
            //    animator.SetInteger("x", Mathf.RoundToInt(actionVector.x));
            //    animator.SetInteger("y", Mathf.RoundToInt(actionVector.y));
            //}
        }
    }

    void OnDestroy()
    {
        Game.controls.Player.Attack.performed -= Attack_performed;
        Game.controls.Player.Attack.canceled -= Attack_canceled;
        Game.controls.Player.Defend.performed -= Defend_performed;
        Game.controls.Player.Defend.canceled -= Defend_canceled;
        Game.controls.Player.Crouch.performed -= GroundPound_performed;
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
        if (Game.slowCameraMovementWhenAttacking) movement.lookSpeed *= lookDamp;
        animator.SetTrigger("slash");
        if (sword.IsSwordMagical()) chargeDelayTimer = chargeDelay;
    }

    private void Attack_canceled(InputAction.CallbackContext context)
    {
        if(sword.IsSwordMagical())
        {
            if (sword.IsFullyCharged())
            {
                if (Game.slowCameraMovementWhenAttacking) movement.lookSpeed *= lookDamp;
                animator.SetTrigger("slash");
            }
            else
            {
                sword.ResetCharge();
            }
        }
    }

    private void Defend_performed(InputAction.CallbackContext obj)
    {
        if (Game.slowCameraMovementWhenDefending) movement.lookSpeed *= lookDamp;
    }

    private void Defend_canceled(InputAction.CallbackContext obj)
    {
        if (Game.slowCameraMovementWhenDefending) movement.lookSpeed = Game.mouseSensitivity;
    }

    private void GroundPound_performed(InputAction.CallbackContext context)
    {
        if (!movement.grounded)
        {
            movement.velocity = Vector3.zero;
            movement.isCrouching = true;
            animator.SetTrigger("slash");
            atkAngle = 0;
            slaming = true;
            slamTimer = 0;
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

    [SerializeField] void ReleaseCharge()
    {
        if(sword.IsFullyCharged())
        {
            var slash = Instantiate(slashProjectile);
            Projectile p = slash.GetComponent<Projectile>();
            slash.transform.position = movement.camera.transform.position + movement.camera.transform.forward;
            Vector3 baseRot = Quaternion.LookRotation(movement.camera.transform.forward).eulerAngles;
            slash.transform.localEulerAngles = baseRot + new Vector3(0, 0, atkAngle - 90);

            p.owner = gameObject;
            p.damage = sword.power * 2;
            p.direction = movement.camera.transform.forward;

            sword.ResetCharge();
        }
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
