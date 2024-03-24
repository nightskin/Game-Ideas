using UnityEngine;

public class BladeSystem : MonoBehaviour
{
    [SerializeField] Transform armPivot;
    [SerializeField] Transform weapon;

    [SerializeField][Range(0, 1)] float actionDamp = 0.5f;
    
    FirstPersonPlayer player;
    public Animator animator;


    float atkAngle = 0;
    Vector2 atkVector = Vector2.zero;


    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();

        player.actions.Slash.performed += Slash_performed;
        player.actions.Slash.canceled += Slash_canceled;

    }

    void Update()
    {
        atkVector = player.actions.Look.ReadValue<Vector2>();
        if (atkVector.magnitude > 0.5f)
        {
            atkAngle = Mathf.Atan2(atkVector.x, -atkVector.y) * 180 / Mathf.PI;
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




    ///Animation Events
    public void StartAttack()
    {
        weapon.GetComponent<PlayerWeapon>().attacking = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
    }
    
    public void EndAttack()
    {
        weapon.GetComponent<PlayerWeapon>().attacking = false;
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
    }

}
