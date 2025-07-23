using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{

    public enum WeaponState
    {
        IDLE,
        ATTACKING,
        DEFENDING,
    }

    [SerializeField] Player player;
    [SerializeField] GameObject impactEffectEnemy;
    [SerializeField] GameObject impactEffectSolid;
    [SerializeField] BoxCollider atkCollider;
    [SerializeField] BoxCollider defCollider;

    WeaponState weaponState;

    public float knockbackForce = 10;
    public float damage = 1;

    public void SetState(WeaponState state)
    {
        weaponState = state;
        if (state == WeaponState.ATTACKING)
        {
            atkCollider.enabled = true;
            defCollider.enabled = false;
        }
        else if (state == WeaponState.DEFENDING)
        {
            atkCollider.enabled = false;
            defCollider.enabled = true;
        }
        else
        {
            atkCollider.enabled = false;
            defCollider.enabled = false;
        }
    }

    void LateUpdate()
    {
        if (weaponState == WeaponState.ATTACKING)
        {
            if (Physics.BoxCast(transform.TransformPoint(atkCollider.center), atkCollider.size, -transform.right, out RaycastHit hit, transform.rotation, 1))
            {
                if (hit.transform.tag == "Enemy")
                {
                    if (impactEffectEnemy) Instantiate(impactEffectEnemy, hit.point, Quaternion.LookRotation(hit.normal));

                    SetState(WeaponState.IDLE);   
                }
            }
        }
    }
    
}
