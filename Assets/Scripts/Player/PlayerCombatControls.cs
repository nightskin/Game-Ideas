using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerCombatControls : MonoBehaviour
{
    public Transform armPivot;
    public PlayerWeapon weapon;

    [SerializeField][Range(0, 1)] float actionDamp = 0.1f;
    float defaultLookSpeed;

    public Vector2 atkVector = Vector2.zero;
    public float atkAngle;


    [SerializeField] LayerMask blockLayer;
    RaycastHit blockedAtack;
    Transform rig;

    public Transform camera;
    public FirstPersonPlayer player;
    public Animator animator;
    
    public bool charging = false;
    public bool attacking = false;
    public bool defending = false;

    void Start()
    {
        defaultLookSpeed = player.lookSpeed;
        if (weapon.trail) 
        { 
            weapon.trail.gameObject.SetActive(false);
        }
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
        camera = transform.root.Find("Camera");
        Transform rig = armPivot.transform.Find("Rig");

        player.actions.Attack.performed += Attack_performed;
        player.actions.Attack.canceled += Attack_canceled;

        player.actions.Defend.performed += Defend_performed;
    }

    void Update()
    {
        if(player.actions.Attack.IsPressed())
        {
            atkVector = player.actions.Look.ReadValue<Vector2>().normalized;
            if (atkVector.magnitude > 0)
            {
                atkAngle = Mathf.Atan2(atkVector.x, -atkVector.y) * 180 / Mathf.PI;
            }
        }
        else if(defending)
        {
            
        }

        if (charging)
        {
            weapon.ChargeWeapon();
        }
    }

    void OnDestroy()
    {
        player.actions.Attack.performed -= Attack_performed;
        player.actions.Attack.canceled -= Attack_canceled;
        player.actions.Defend.performed -= Defend_performed;
    }
    
    private void Attack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        charging = true;
        animator.SetTrigger("slash");
    }

    private void Attack_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(weapon.FullyCharged())
        {
            animator.SetTrigger("slash");
        }
        else
        {
            weapon.LoseCharge();
        }
        charging = false;
    }

    private void Defend_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    ///Animation Events
    public void StartSlash()
    {
        attacking = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        if(weapon.trail)
        {
            weapon.trail.gameObject.SetActive(true);
        }
        player.lookSpeed *= actionDamp;
    }
    
    public void EndSlash()
    {
        attacking = false;
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        if (weapon.trail)
        {
            weapon.trail.gameObject.SetActive(false);
        }
        player.lookSpeed = defaultLookSpeed;
    }
    
    public void ChargeSlash()
    {
        if (weapon.FullyCharged())
        {
            weapon.ReleaseCharge();
            weapon.LoseCharge();
        }
    }
}
