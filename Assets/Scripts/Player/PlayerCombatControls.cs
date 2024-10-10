using UnityEngine;
using UnityEngine.UI;

public class PlayerCombatControls : MonoBehaviour
{
    public enum Weapon
    {
        SWORD,
        GUN,
    }
    public Weapon equipedWeapon;

    [SerializeField] Image reticle;
    [SerializeField] LayerMask lockOnLayer;

    public Transform armPivot;
    public PlayerSword sword;
    public PlayerGun gun;


    [SerializeField][Range(0, 1)] float actionDamp = 0.1f;
    float defaultLookSpeed;

    public Vector2 atkVector = Vector2.zero;
    public float atkAngle = 90;

    public FirstPersonPlayer player;
    public Animator animator;


    public bool hasSword;
    bool charging = false;

    void Start()
    {
        Equip(equipedWeapon);

        defaultLookSpeed = player.lookSpeed;
        if (sword.trail) 
        { 
            sword.trail.gameObject.SetActive(false);
        }
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();

        player.actions.Attack.performed += Attack_performed;
        player.actions.Attack.canceled += Attack_canceled;
        player.actions.ToggleWeapons.performed += ToggleWeapons_performed;

    }

    void FixedUpdate()
    {
        if(reticle)
        {
            Ray ray = player.camera.ScreenPointToRay(reticle.rectTransform.position);
            if(Physics.Raycast(ray,player.camera.farClipPlane,lockOnLayer))
            {
                reticle.color = Color.red;
            }
            else
            {
                reticle.color = Color.white;
            }
        }
    }

    void Update()
    {
        atkVector = player.actions.Look.ReadValue<Vector2>().normalized;
        if (player.actions.Attack.IsPressed())
        {
            if(equipedWeapon == Weapon.SWORD)
            {
                if (atkVector.magnitude > 0)
                {
                    atkAngle = Mathf.Atan2(atkVector.x, -atkVector.y) * 180 / Mathf.PI;
                }
                else
                {
                    atkAngle = Random.Range(-180, 180);
                }
            }
            else if(equipedWeapon == Weapon.GUN)
            {
                if(gun.shootStyle == PlayerGun.ShootStyle.RAPID_FIRE)
                {
                    gun.RapidFire();
                }
                else if(gun.shootStyle == PlayerGun.ShootStyle.CHARGE_BLAST)
                {
                    gun.Charge();
                }
            }
        }

        if (charging)
        {
            sword.ChargeWeapon();
        }
    }

    void OnDestroy()
    {
        player.actions.Attack.performed -= Attack_performed;
        player.actions.Attack.canceled -= Attack_canceled;
        player.actions.ToggleWeapons.performed -= ToggleWeapons_performed;
    }
    
    private void Attack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        charging = true;
        if(equipedWeapon == Weapon.SWORD)
        {
            animator.SetTrigger("slash");
        }
        else if(equipedWeapon == Weapon.GUN)
        {
            gun.Fire();
            gun.shooting = true;
        }
    }

    private void Attack_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(equipedWeapon == Weapon.SWORD)
        {
            if (sword.FullyCharged())
            {
                if (equipedWeapon == Weapon.SWORD)
                {
                    animator.SetTrigger("slash");
                }
                else if (equipedWeapon == Weapon.GUN)
                {
                    animator.SetTrigger("shoot");
                }
            }
            else
            {
                sword.LoseCharge();
            }
            charging = false;
        }
        else if(equipedWeapon == Weapon.GUN)
        {
            if(gun.shootStyle == PlayerGun.ShootStyle.RAPID_FIRE)
            {
                gun.shooting = false;
            }
            else if(gun.shootStyle == PlayerGun.ShootStyle.CHARGE_BLAST)
            {
                gun.ReleaseCharge();
            }
        }
    }
    
    private void ToggleWeapons_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(equipedWeapon == Weapon.SWORD)
        {
            equipedWeapon = Weapon.GUN;

        }
        else if(equipedWeapon == Weapon.GUN)
        {
            equipedWeapon = Weapon.SWORD;

        }
        Equip(equipedWeapon);
    }
    
    void Equip(Weapon weapon)
    {
        if(weapon == Weapon.SWORD && hasSword)
        {
            sword.transform.parent.gameObject.SetActive(true);
            gun.gameObject.SetActive(false);
        }
        else if(weapon == Weapon.GUN)
        {
            sword.transform.parent.gameObject.SetActive(false);
            gun.gameObject.SetActive(true);
        }
        equipedWeapon = weapon;
    }

    ///Animation Events
    
    public void StartSlash()
    {
        sword.slashing = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        if(sword.trail)
        {
            sword.trail.gameObject.SetActive(true);
        }
        player.lookSpeed *= actionDamp;
    }
    
    public void EndSlash()
    {
        sword.slashing = false;
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        if (sword.trail)
        {
            sword.trail.gameObject.SetActive(false);
        }
        player.lookSpeed = defaultLookSpeed;
    }
    
    public void ChargeSlash()
    {
        if (sword.FullyCharged())
        {
            sword.ReleaseCharge();
            sword.LoseCharge();
        }
    }
}
