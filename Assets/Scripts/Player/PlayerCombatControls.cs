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
    public PlayerMovement movementControls;
    public PlayerSword sword;
    [SerializeField][Range(0, 0.999f)] float triggerThreshold = 0.5f;
    [SerializeField][Range(0, 1)] float actionDamp = 0.1f;
    float atkAngle = 0;
    [HideInInspector] public bool stunned = false;

    void Start()
    {
        Game.controls.Player.Attack.performed += Attack_performed;
        Game.controls.Player.Defend.performed += Defend_performed;
        Game.controls.Player.Defend.canceled += Defend_canceled;
    }

    void Update()
    {
        Vector2 actionVector = Game.controls.Player.Look.ReadValue<Vector2>();
        if (Game.controls.Player.Defend.ReadValue<float>() > triggerThreshold)
        {
            animator.SetInteger("x", Mathf.RoundToInt(actionVector.x));
            animator.SetInteger("y", Mathf.RoundToInt(actionVector.y));
        }
        if (Game.controls.Player.Attack.ReadValue<float>() > triggerThreshold)
        {
            atkAngle = Mathf.Atan2(actionVector.x, -actionVector.y) * 180 / Mathf.PI;
        }
    }

    void OnDestroy()
    {
        Game.controls.Player.Attack.performed -= Attack_performed;
        Game.controls.Player.Defend.performed -= Defend_performed;
        Game.controls.Player.Defend.canceled -= Defend_canceled;
    }

    private void Attack_performed(InputAction.CallbackContext obj)
    {
        if (obj.ReadValue<float>() > triggerThreshold)
        {
            if (Game.slowCameraMovementWhenAttacking) movementControls.lookSpeed *= actionDamp;
            animator.SetTrigger("slash");
        }
    }

    private void Defend_performed(InputAction.CallbackContext obj)
    {
        if (obj.ReadValue<float>() > triggerThreshold)
        {
            if (Game.slowCameraMovementWhenDefending) movementControls.lookSpeed *= actionDamp;
        }
    }

    private void Defend_canceled(InputAction.CallbackContext obj)
    {
        if (Game.slowCameraMovementWhenDefending) movementControls.lookSpeed = Game.mouseSensitivity;
    }

    //Animation Events
    [SerializeField]
    void TurnOnTrail()
    {
        if (!sword.trail.gameObject.activeSelf) sword.trail.gameObject.SetActive(true);
    }

    [SerializeField] void TurnOffTrail()
    {
        if (sword.trail.gameObject.activeSelf) sword.trail.gameObject.SetActive(false);
        for (int t = 0; t < sword.trail.positionCount; t++)
        {
            sword.trail.SetPosition(t, Vector3.zero);
        }
    }

    [SerializeField] void Reset()
    {
        if(state != PlayerCombatState.IDLE) state = PlayerCombatState.IDLE;
    }

    [SerializeField] void StartSlash()
    {
        state = PlayerCombatState.ATK;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        animator.SetInteger("x", 0);
        animator.SetInteger("y", 0);
    }

    [SerializeField] void EndSlash()
    {
        state = PlayerCombatState.IDLE;
        armPivot.localEulerAngles = Vector3.zero;
        movementControls.lookSpeed = Game.mouseSensitivity;
    }

    [SerializeField] void Block()
    {
        state = PlayerCombatState.DEF;
        armPivot.localEulerAngles = Vector3.zero;
    }
}
