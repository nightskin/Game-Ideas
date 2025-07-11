using UnityEngine;

public class PlayerWeapon: MonoBehaviour
{

    [SerializeField] Player player;
    [SerializeField] GameObject impactEffectEnemy;
    [SerializeField] GameObject impactEffectSolid;


    [HideInInspector] public bool slashing = false;
    public float knockbackForce = 10;
    public float damage = 1;



    void OnTriggerEnter(Collider other)
    {
        if (slashing)
        {
            if (other.transform.tag == "Solid" || other.tag == "EnemyWeapon")
            {
                if(impactEffectSolid) Instantiate(impactEffectSolid, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                player.animator.SetTrigger("recoil");
                slashing = false;
            }
            else if (other.transform.tag == "Enemy")
            {
                if(impactEffectEnemy) Instantiate(impactEffectEnemy, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                if (other.transform.root.GetComponent<HealthScript>())
                {
                    HealthScript health = other.transform.root.GetComponent<HealthScript>();
                    health.TakeDamage(damage);
                    if(health.IsDead())
                    {
                        
                    }
                }
            }
        }
    }
}
