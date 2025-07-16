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
    BoxCollider boxCollider;

    [HideInInspector] public WeaponState state;
    public float knockbackForce = 10;
    public float damage = 1;

    void Start()
    {
        boxCollider = transform.GetComponent<BoxCollider>();
    }


    void OnTriggerEnter(Collider hit)
    {
        if (state == WeaponState.ATTACKING)
        {
            if (hit.tag == "Solid" || hit.tag == "EnemyWeapon")
            {
                if (impactEffectSolid) Instantiate(impactEffectSolid, hit.ClosestPointOnBounds(transform.position), Quaternion.identity);
                player.animator.SetTrigger("recoil");
            }
            else if (hit.tag == "Enemy")
            {
                if (impactEffectEnemy) Instantiate(impactEffectEnemy, hit.ClosestPointOnBounds(transform.position), Quaternion.identity);
            }
        }
        else if (state == WeaponState.DEFENDING)
        {
            
        }
    }
}
