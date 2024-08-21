using System.Data;
using UnityEngine;

public class BladeSystem : MonoBehaviour
{
    public Transform armPivot;
    public PlayerWeapon weapon;

    [SerializeField][Range(0, 1)] float actionDamp = 0.1f;
    float defaultLookSpeed;

    public Vector2 atkVector = Vector2.zero;
    public float atkAngle;

    public FirstPersonPlayer player;
    public Animator animator;
    
    bool blocking = false;
    bool charging = false;

    void Start()
    {
        defaultLookSpeed = player.lookSpeed;
        if (weapon.trail) 
        { 
            weapon.trail.gameObject.SetActive(false);
        }
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
        
        player.actions.Slash.performed += Slash_performed;
        player.actions.Slash.canceled += Slash_canceled;
        player.actions.Defend.performed += Defend_performed;
        player.actions.Defend.canceled += Defend_canceled;
    }
    
    void Update()
    {
        if(player.actions.Slash.IsPressed())
        {
            atkVector = player.actions.Look.ReadValue<Vector2>().normalized;
            if (atkVector.magnitude > 0)
            {
                atkAngle = Mathf.Atan2(atkVector.x, -atkVector.y) * 180 / Mathf.PI;
            }
        }
        
        if (charging)
        {
            weapon.ChargeWeapon();
        }
        if (blocking)
        {
            animator.SetInteger("blockX", Mathf.RoundToInt(player.actions.Look.ReadValue<Vector2>().x));
            animator.SetInteger("blockY", Mathf.RoundToInt(player.actions.Look.ReadValue<Vector2>().y));
        }
    }

    void OnDestroy()
    {
        player.actions.Slash.performed -= Slash_performed;
        player.actions.Slash.canceled -= Slash_canceled;
        player.actions.Defend.performed -= Defend_performed;
        player.actions.Defend.canceled -= Defend_canceled;
    }

    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        charging = true;
        weapon.defending = false;
        animator.SetTrigger("slash");
    }

    private void Slash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(weapon.FullyCharged())
        {
            weapon.defending = false;
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
        blocking = true;
        weapon.defending = true;
        player.lookSpeed *= actionDamp;
    }

    private void Defend_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        blocking = false;
        weapon.defending = false;
        player.lookSpeed = defaultLookSpeed;
    }
    
    ///Animation Events
    public void StartSlash()
    {
        weapon.attacking = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        if(weapon.trail)
        {
            weapon.trail.gameObject.SetActive(true);
        }

    }
    
    public void EndSlash()
    {
        weapon.attacking = false;
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        if (weapon.trail)
        {
            weapon.trail.gameObject.SetActive(false);
        }

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
