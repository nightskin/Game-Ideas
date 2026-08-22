using UnityEngine;

public class MeleeCombat : MonoBehaviour
{
    public Animator animator;
    public CharacterMovement basicMovement;
    public Transform armPivot;
    float atkAngle = 0;
    Vector2 atkVector = Vector2.zero;
    Vector2 defAngle = Vector2.zero;

    public enum CombatState
    {
        IDLE,
        ATTACKING,
        DEFENDING,
    }
    CombatState state;


    void Start()
    {
        if(!basicMovement) basicMovement = GetComponent<CharacterMovement>();
        if(!animator) animator = GetComponent<Animator>();
    }

    void Update()
    {
        atkVector = Game.input.Player.Look.ReadValue<Vector2>();
        
        if(Game.input.Player.Attack.WasPerformedThisFrame())
        {
            atkAngle = Mathf.Atan2(atkVector.x, -atkVector.y) * 180 / Mathf.PI;
            animator.SetTrigger("cut");
        }
        
        if(Game.input.Player.Defend.IsPressed())
        {
            
        }
    }
    
    //Animation events
    public void AttackState()
    {
        armPivot.localEulerAngles = new Vector3(0,0,atkAngle);
        state = CombatState.ATTACKING;
    }
    public void DefendState()
    {
        armPivot.localEulerAngles = Vector3.zero;
        state = CombatState.DEFENDING;
    }
    public void IdleState()
    {
        armPivot.localEulerAngles = Vector3.zero;
        state = CombatState.IDLE;
    }
}
