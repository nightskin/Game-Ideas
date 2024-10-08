using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] PlayerCombatControls combatControls;
    [SerializeField] Transform bulletSpawn;

    public float knockbackForce = 10;
    public float damage = 1;
    
    void Start()
    {
        if(!combatControls) combatControls = transform.root.GetComponent<PlayerCombatControls>();
        if(!bulletSpawn) bulletSpawn = transform.GetChild(0);
    }


    public void Fire()
    {
        var b = Instantiate(bullet, bulletSpawn.position, Quaternion.identity);
        Projectile projectile = b.GetComponent<Projectile>();
        if (projectile)
        {
            projectile.owner = transform.root.gameObject;

            if(Physics.Raycast(combatControls.player.camera.transform.position, combatControls.player.camera.transform.forward, out RaycastHit hit))
            {
                projectile.direction = (hit.point - bulletSpawn.position).normalized;
            }
            else
            {
                projectile.direction = combatControls.player.camera.transform.forward;
            }
        }
    }

}
