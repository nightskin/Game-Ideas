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
    [SerializeField] PlayerMovement player;

    [SerializeField][Range(0, 1)] float actionDamp = 0.1f;
    float atkAngle = 0;
    [HideInInspector] public bool stunned = false;

    void Start()
    {
        player.actions.Attack.performed += Attack_performed;
        player.actions.Defend.performed += Defend_performed;
        player.actions.Defend.canceled += Defend_canceled;
    }

    void Update()
    {
        Vector2 actionVector = player.actions.Look.ReadValue<Vector2>();
        if (player.actions.Defend.IsPressed())
        {
            animator.SetInteger("x", Mathf.RoundToInt(actionVector.x));
            animator.SetInteger("y", Mathf.RoundToInt(actionVector.y));
        }
        if (player.actions.Attack.IsPressed())
        {
            atkAngle = Mathf.Atan2(actionVector.x, -actionVector.y) * 180 / Mathf.PI;
        }
    }

    void OnDestroy()
    {
        player.actions.Attack.performed -= Attack_performed;
        player.actions.Defend.performed -= Defend_performed;
        player.actions.Defend.canceled -= Defend_canceled;
    }

    private void Attack_performed(InputAction.CallbackContext obj)
    {
        if (GameSettings.slowCameraMovementWhenAttacking) player.lookSpeed *= actionDamp;
        animator.SetTrigger("slash");
    }

    private void Defend_performed(InputAction.CallbackContext obj)
    {
        if (GameSettings.slowCameraMovementWhenDefending) player.lookSpeed *= actionDamp;
    }

    private void Defend_canceled(InputAction.CallbackContext obj)
    {
        if (GameSettings.slowCameraMovementWhenDefending) player.lookSpeed = GameSettings.aimSensitivity;
    }

    //Animation Events
    public void Reset()
    {
        state = PlayerSwordState.IDLE;
    }

    public void StartSlash()
    {
        state = PlayerSwordState.ATK;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        animator.SetInteger("x", 0);
        animator.SetInteger("y", 0);
    }

    public void EndSlash()
    {
        state = PlayerSwordState.IDLE;
        armPivot.localEulerAngles = Vector3.zero;
        player.lookSpeed = GameSettings.aimSensitivity;
    }

    public void Thrust()
    {
        state = PlayerSwordState.ATK;
        armPivot.localEulerAngles = Vector3.zero;
        animator.SetInteger("x", 0);
        animator.SetInteger("y", 0);
    }

    public void Block()
    {
        state = PlayerSwordState.DEF;
        armPivot.localEulerAngles = Vector3.zero;
    }
}
