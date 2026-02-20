using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatControls : MonoBehaviour
{
    public enum CombatState
    {
        IDLE,
        ATK,
        DEF,
    }
    [HideInInspector] public CombatState state = CombatState.IDLE;
    [SerializeField] GameObject slashProjectile;
    public Animator animator;
    public Transform armPivot;
    public PlayerMovement movement;
    public PlayerSword sword;
    [SerializeField][Range(0, 1)] float lookDamp = 0.1f;
    Vector2 actionVector = Vector2.zero;
    Vector2 defVector = Vector2.zero;
    float atkAngle;


    // charging Variables
    float chargeDelayTimer;
    [SerializeField] float chargeDelay = 0.25f; 

    //Stun Variables For When The Player is Hit
    [HideInInspector] public bool wasHit = false;
    [HideInInspector] public Vector3 knockBackForce;
    [Range(0, 1)] float stunTime = 0.05f;
    float stunTimer = 0;


    void Start()
    {
        chargeDelayTimer = 0;
        stunTimer = stunTime;
        Game.controls.Player.Attack.performed += Attack_performed;
        Game.controls.Player.Attack.canceled += Attack_canceled;
        Game.controls.Player.Defend.performed += Defend_performed;
        Game.controls.Player.Defend.canceled += Defend_canceled;
    }

    void Update()
    {
        if(wasHit)
        {
            
        }
        else
        {
            actionVector = Game.controls.Player.Look.ReadValue<Vector2>();
            if (Game.controls.Player.Attack.IsPressed())
            {
                atkAngle = Mathf.Atan2(actionVector.x, -actionVector.y) * 180 / Mathf.PI;
                if (sword.magical)
                {
                    chargeDelayTimer -= Time.deltaTime;
                    if (chargeDelayTimer <= 0)
                    {
                        animator.SetBool("charging", true);
                    }
                }
            }
            else if (Game.controls.Player.Defend.IsPressed())
            {
                defVector.x += actionVector.x * 20 * Time.deltaTime;
                defVector.y += actionVector.y * 20 * Time.deltaTime;

                defVector.x = Mathf.Clamp(defVector.x, -1, 1);
                defVector.y = Mathf.Clamp01(defVector.y);

                animator.SetFloat("x", defVector.x);
                animator.SetFloat("y", defVector.y);
            }
        }
    }

    void OnDestroy()
    {
        Game.controls.Player.Attack.performed -= Attack_performed;
        Game.controls.Player.Attack.canceled -= Attack_canceled;
        Game.controls.Player.Defend.performed -= Defend_performed;
        Game.controls.Player.Defend.canceled -= Defend_canceled;
    }

    private void Attack_performed(InputAction.CallbackContext obj)
    {
        if (Game.slowCameraMovementWhenAttacking) movement.lookSpeed *= lookDamp;
        animator.SetTrigger("slash");
        if (sword.magical) chargeDelayTimer = chargeDelay;
    }

    private void Attack_canceled(InputAction.CallbackContext context)
    {
        if(sword.magical && sword.fullyCharged)
        {
            animator.SetTrigger("slash");
        }
    }

    private void Defend_performed(InputAction.CallbackContext obj)
    {
        if (Game.slowCameraMovementWhenDefending) movement.lookSpeed *= lookDamp;
        animator.SetBool("blocking", true);
    }

    private void Defend_canceled(InputAction.CallbackContext obj)
    {
        if (Game.slowCameraMovementWhenDefending) movement.lookSpeed = Game.aimSense;
        animator.SetBool("blocking", false);
    }
    
    //Animation Events
    public void StartSlash()
    {
        state = CombatState.ATK;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        StartCoroutine(sword.AnimateTrail());
        sword.chargeEffect.SetActive(false);

    }
    public void EndSlash()
    {
        state = CombatState.IDLE;
        defVector = Vector2.zero;
        movement.lookSpeed = Game.aimSense;
        armPivot.localEulerAngles = Vector3.zero;
    }
    public void ChargeSword()
    {
        sword.chargeEffect.SetActive(true);
        sword.fullyCharged = true;
    }
    public void ReleaseCharge()
    {
        if(sword.fullyCharged && sword.magical)
        {
            sword.fullyCharged = false;
            animator.SetBool("charging", false);

            var slash = Instantiate(slashProjectile);
            Projectile p = slash.GetComponent<Projectile>();
            slash.transform.position = movement.camera.transform.position + movement.camera.transform.forward;
            Vector3 baseRot = Quaternion.LookRotation(movement.camera.transform.forward).eulerAngles;
            slash.transform.localEulerAngles = baseRot + new Vector3(0, 0, atkAngle - 90);

            p.owner = gameObject;
            p.damage = sword.power * 2;
            p.direction = movement.camera.transform.forward;
        }
    }
    public void StartBlock()
    {
        state = CombatState.DEF;
        armPivot.localEulerAngles = Vector3.zero;
    }
    public void BackToIdle()
    {
        state = CombatState.IDLE;
        sword.fullyCharged = false;
        sword.chargeEffect.SetActive(false);
        animator.SetBool("charging", false);
    }
}
