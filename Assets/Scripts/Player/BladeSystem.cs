using UnityEngine;

public class BladeSystem : MonoBehaviour
{
    [SerializeField] Transform armPivot;
    [SerializeField] PlayerWeapon weapon;
    [SerializeField] ParticleSystem weaponTrail;

    [SerializeField][Range(0, 1)] float actionDamp = 0.5f;
    float defaultLookSpeed;
    
    FirstPersonPlayer player;
    public Animator animator;


    float atkAngle = 0;
    public Vector2 atkVector = Vector2.zero;
    bool blocking = false;

    void Start()
    {
        if (weaponTrail) 
        { 
            weaponTrail.gameObject.SetActive(false);
            weaponTrail.startColor = weapon.trailColor;
        }
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
        defaultLookSpeed = player.lookSpeed;
        
        player.actions.Slash.performed += Slash_performed;
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
            else
            {
                atkAngle = Random.Range(-180, 180);
            }
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
        player.actions.Defend.performed -= Defend_performed;
        player.actions.Defend.canceled -= Defend_canceled;
    }

    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        weapon.defending = false;
        animator.SetTrigger("slash");
    }
    
    private void Defend_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        blocking = true;
        weapon.defending = true;
        player.lookSpeed = player.lookSpeed * actionDamp;
    }

    private void Defend_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        blocking = false;
        player.lookSpeed = defaultLookSpeed;
    }



    ///Animation Events
    public void StartSlash()
    {
        weapon.attacking = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        player.lookSpeed = player.lookSpeed * actionDamp;
        if(weaponTrail)
        {
            weaponTrail.gameObject.SetActive(true);
        }

    }
    
    public void EndSlash()
    {
        weapon.attacking = false;
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        player.lookSpeed = defaultLookSpeed;
        if(weaponTrail)
        {
            weaponTrail.gameObject.SetActive(false);
        }

    }

}
