using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{


    [SerializeField] Player player;
    [SerializeField] GameObject impactEffectEnemy;
    [SerializeField] GameObject impactEffectSolid;
    [SerializeField] BoxCollider collider;

    public float knockbackForce = 10;
    public float damage = 1;


    void Update()
    {
        if (player.state == Player.PlayerCombatState.ATK)
        {
            if (Physics.BoxCast(transform.TransformPoint(collider.center), collider.size, -transform.right, out RaycastHit hit, transform.rotation, 1))
            {
                if (hit.transform.tag == "Enemy")
                {
                    if (impactEffectEnemy) Instantiate(impactEffectEnemy, hit.point, Quaternion.LookRotation(hit.normal));

                    
                }
            }
        }
    }
    
}
