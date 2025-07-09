using UnityEngine;


public class PlayerCombatControls : MonoBehaviour
{
    public Transform armPivot;
    public PlayerMeleeWeapon weapon;

    [SerializeField][Range(0, 1)] float actionDamp = 0.1f;
    float defaultLookSpeed;

    Vector2 actionVector = Vector2.zero;
    float atkAngle = 180;

    public PlayerMovement player;
    public Animator animator;

    void Start()
    {
        defaultLookSpeed = player.lookSpeed;
        animator = GetComponent<Animator>();
        player = GetComponent<PlayerMovement>();

        player.actions.Attack.performed += Slash_performed;

    }
    
    void Update()
    {
        actionVector = player.actions.Look.ReadValue<Vector2>().normalized;
        if (player.actions.Attack.IsPressed())
        {
            if (actionVector.magnitude > 0)
            {
                atkAngle = Mathf.Atan2(actionVector.x, -actionVector.y) * 180 / Mathf.PI;
            }
        }
        //else if(player.actions.Defend.IsPressed())
        //{
        //    if (actionVector.magnitude > 0)
        //    {
        //        animator.SetInteger("defX", Mathf.RoundToInt(actionVector.x));
        //        animator.SetInteger("defY", Mathf.RoundToInt(actionVector.y));
        //    }
        //}
    }

    void OnDestroy()
    {
        player.actions.Attack.performed -= Slash_performed;
    }
    
    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetInteger("defX", 0);
        animator.SetInteger("defY", 0);
        animator.SetTrigger("slash");
    }

    
    
    ///Animation Events

    public void StartSlash()
    {
        weapon.slashing = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        player.lookSpeed = player.lookSpeed * actionDamp;
    }
    
    public void EndSlash()
    {
        weapon.slashing = false;
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        player.lookSpeed = defaultLookSpeed;
    }
    
}
