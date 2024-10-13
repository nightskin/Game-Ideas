using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombatControls : MonoBehaviour
{
    public enum Weapons
    {
        GUN_DEFAULT,
        GUN_CHARGE_BLAST,
        SWORD,
    }
    public List<Weapons> inventory;
    int equipIndex = 0;

    public Image reticle;
    public LayerMask lockOnLayer;
    public RaycastHit lockOnHit;

    public Transform armPivot;
    public PlayerSword sword;
    public PlayerGun gun;


    [SerializeField][Range(0, 1)] float actionDamp = 0.1f;
    float defaultLookSpeed;

    public Vector2 atkVector = Vector2.zero;
    public float atkAngle = 90;

    public PlayerMovement player;
    public Animator animator;

    void Start()
    {
        Equip(inventory[equipIndex]);

        defaultLookSpeed = player.lookSpeed;
        if (sword.trail) 
        { 
            sword.trail.gameObject.SetActive(false);
        }
        animator = GetComponent<Animator>();
        player = GetComponent<PlayerMovement>();

        player.actions.Attack.performed += Attack_performed;
        player.actions.Attack.canceled += Attack_canceled;
        player.actions.ToggleWeapons.performed += ToggleWeapons_performed;

    }

    void FixedUpdate()
    {
        if(reticle)
        {
            Ray ray = new Ray(player.camera.transform.position, player.camera.transform.forward);
            if(Physics.Raycast(ray, out lockOnHit ,player.camera.farClipPlane,lockOnLayer))
            {
                if(!lockOnHit.collider.isTrigger)
                {
                    reticle.color = Color.red;
                }
                else
                {
                    reticle.color = Color.white;
                }
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
            if(inventory[equipIndex] == Weapons.SWORD)
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
            else
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

    }

    void OnDestroy()
    {
        player.actions.Attack.performed -= Attack_performed;
        player.actions.Attack.canceled -= Attack_canceled;
        player.actions.ToggleWeapons.performed -= ToggleWeapons_performed;
    }
    
    private void Attack_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(inventory[equipIndex] == Weapons.SWORD)
        {
            animator.SetTrigger("slash");
        }
        else
        {
            gun.Fire();
            gun.shooting = true;
        }
    }

    private void Attack_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (inventory[equipIndex] != Weapons.SWORD)
        {
            if (gun.shootStyle == PlayerGun.ShootStyle.RAPID_FIRE)
            {
                gun.shooting = false;
            }
            else if (gun.shootStyle == PlayerGun.ShootStyle.CHARGE_BLAST)
            {
                gun.ReleaseCharge();
            }
        }
    }
    
    private void ToggleWeapons_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(obj.ReadValue<float>() > 0)
        {
            if(equipIndex < inventory.Count - 1) 
            {
                equipIndex++;
            }
            else
            {
                equipIndex = 0;
            }
        }
        else if(obj.ReadValue<float>() < 0)
        {
            if (equipIndex > 0)
            {
                equipIndex--;
            }
            else
            {
                equipIndex = inventory.Count - 1;
            }
        }
        Equip(inventory[equipIndex]);
    }
    
    void Equip(Weapons weapon)
    {
        if(weapon == Weapons.SWORD)
        {
            sword.transform.parent.gameObject.SetActive(true);
            gun.gameObject.SetActive(false);
        }
        else if(weapon == Weapons.GUN_DEFAULT)
        {
            sword.transform.parent.gameObject.SetActive(false);
            gun.gameObject.SetActive(true);
            gun.shootStyle = PlayerGun.ShootStyle.RAPID_FIRE;
            gun.GetComponent<MeshRenderer>().materials[3].color = Color.blue;
        }
        else if(weapon == Weapons.GUN_CHARGE_BLAST)
        {
            sword.transform.parent.gameObject.SetActive(false);
            gun.gameObject.SetActive(true);
            gun.shootStyle = PlayerGun.ShootStyle.CHARGE_BLAST;
            gun.GetComponent<MeshRenderer>().materials[3].color = new Color(0.5f, 0, 1);
        }
        inventory[equipIndex] = weapon;
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
    
}
