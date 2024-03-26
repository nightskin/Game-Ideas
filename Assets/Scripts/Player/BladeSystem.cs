using UnityEngine;

public class BladeSystem : MonoBehaviour
{
    [SerializeField] Transform armPivot;
    [SerializeField] PlayerWeapon weapon;

    [SerializeField][Range(0, 1)] float actionDamp = 0.5f;
    float defaultLookSpeed;
    
    FirstPersonPlayer player;
    public Animator animator;


    float atkAngle = 0;
    public Vector2 atkVector = Vector2.zero;

    bool defending = false;


    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
        defaultLookSpeed = player.lookSpeed;

        player.actions.Slash.performed += Slash_performed;
        player.actions.Slash.canceled += Slash_canceled;
        player.actions.Defend.performed += Defend_performed;
        player.actions.Defend.canceled += Defend_canceled;

    }
    
    void Update()
    {
        atkVector = player.actions.Look.ReadValue<Vector2>();
        if (atkVector.magnitude > 0)
        {
            atkAngle = Mathf.Atan2(atkVector.x, -atkVector.y) * 180 / Mathf.PI;
        }

        if (defending)
        {
            animator.SetInteger("blockX", Mathf.RoundToInt(player.actions.Look.ReadValue<Vector2>().x));
            animator.SetInteger("blockY", Mathf.RoundToInt(player.actions.Look.ReadValue<Vector2>().y));
        }
    }

    void OnDestroy()
    {
        player.actions.Slash.performed -= Slash_performed;
        player.actions.Slash.canceled -= Slash_canceled;
    }

    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetTrigger("atk");
    }

    private void Slash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    private void Defend_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        defending = true;
    }

    private void Defend_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        defending = false;
    }




    ///Animation Events
    public void StartAttack()
    {
        weapon.attacking = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        player.lookSpeed *= actionDamp;
    }
    
    public void EndAttack()
    {
        weapon.attacking = false;
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        player.lookSpeed = defaultLookSpeed;
    }

}
