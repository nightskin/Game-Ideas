using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatControls : MonoBehaviour
{

    public enum PlayerSwordState
    {
        IDLE,
        ATK,
        DEF,
    }
    [HideInInspector] public PlayerSwordState state = PlayerSwordState.IDLE;
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
        movementControls.actions.Attack.performed += Attack_performed;
        movementControls.actions.Defend.performed += Defend_performed;
        movementControls.actions.Defend.canceled += Defend_canceled;
    }

    void Update()
    {
        Vector2 actionVector = movementControls.actions.Look.ReadValue<Vector2>();
        if (movementControls.actions.Defend.ReadValue<float>() > triggerThreshold)
        {
            animator.SetInteger("x", Mathf.RoundToInt(actionVector.x));
            animator.SetInteger("y", Mathf.RoundToInt(actionVector.y));
        }
        if (movementControls.actions.Attack.ReadValue<float>() > triggerThreshold)
        {
            atkAngle = Mathf.Atan2(actionVector.x, -actionVector.y) * 180 / Mathf.PI;
        }
    }

    void OnDestroy()
    {
        movementControls.actions.Attack.performed -= Attack_performed;
        movementControls.actions.Defend.performed -= Defend_performed;
        movementControls.actions.Defend.canceled -= Defend_canceled;
    }

    private void Attack_performed(InputAction.CallbackContext obj)
    {
        if (obj.ReadValue<float>() > triggerThreshold)
        {
            if (GameSettings.slowCameraMovementWhenAttacking) movementControls.lookSpeed *= actionDamp;
            animator.SetTrigger("slash");
        }
    }

    private void Defend_performed(InputAction.CallbackContext obj)
    {
        if (obj.ReadValue<float>() > triggerThreshold)
        {
            if (GameSettings.slowCameraMovementWhenDefending) movementControls.lookSpeed *= actionDamp;
        }
    }

    private void Defend_canceled(InputAction.CallbackContext obj)
    {
        if (GameSettings.slowCameraMovementWhenDefending) movementControls.lookSpeed = GameSettings.aimSensitivity;
    }

    //Animation Events
    [SerializeField] void TurnOnTrail()
    {
        if (!sword.trailOn) sword.trailOn = true;
    }

    [SerializeField] void TurnOffTrail()
    {
        if (sword.trailOn) sword.trailOn = false;

        for (int t = 0; t < sword.trail.positionCount; t++)
        {
            sword.trail.SetPosition(t, Vector3.zero);
        }

    }

    [SerializeField] void Reset()
    {
        state = PlayerSwordState.IDLE;
    }

    [SerializeField] void StartSlash()
    {
        state = PlayerSwordState.ATK;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        animator.SetInteger("x", 0);
        animator.SetInteger("y", 0);
    }

    [SerializeField] void EndSlash()
    {
        state = PlayerSwordState.IDLE;
        armPivot.localEulerAngles = Vector3.zero;
        movementControls.lookSpeed = GameSettings.aimSensitivity;
    }

    [SerializeField] void Block()
    {
        state = PlayerSwordState.DEF;
        armPivot.localEulerAngles = Vector3.zero;
    }
}
