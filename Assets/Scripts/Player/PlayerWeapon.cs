using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] SwordPlayer player;
    [SerializeField] GameObject hitEffect;
    [SerializeField] BoxCollider collider;

    public float knockbackForce = 10;
    public float damage = 1;


    void Update()
    {
        if (player.state == SwordPlayer.PlayerCombatState.ATK)
        {
            if (Physics.BoxCast(transform.TransformPoint(collider.center), collider.size, -transform.right, out RaycastHit hit, transform.rotation, 1))
            {
                if (hit.transform.tag == "Enemy")
                {
                    if (hitEffect) Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                }
            }
        }
    }
    
}
