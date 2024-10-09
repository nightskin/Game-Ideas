using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] PlayerCombatControls combatControls;
    [SerializeField] Transform bulletSpawn;
    
    public float baseDamage = 1;
    public bool shooting = false;
   
    public enum AltFireType
    {
        RAPID_FIRE,
        CHARGE_BLAST
    }
    public AltFireType altFireType;

    [Header("Rapid Fire Settings")]
    public float fireRate = 0.1f;
    float shootTimer = 0;

    [Header("Charge Blast Settings")]
    public float maxChargeTime = 1;
    float chargeTimer = 0;

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
            projectile.damage = baseDamage;
            if (Physics.Raycast(combatControls.player.camera.transform.position, combatControls.player.camera.transform.forward, out RaycastHit hit))
            {
                projectile.direction = (hit.point - bulletSpawn.position).normalized;
            }
            else
            {
                projectile.direction = combatControls.player.camera.transform.forward;
            }
        }
        shootTimer = fireRate;
    }

    public void RapidFire()
    {
        if(altFireType == AltFireType.RAPID_FIRE)
        {
            if (shootTimer > 0)
            {
                shootTimer -= Time.deltaTime;
            }
            else
            {
                var b = Instantiate(bullet, bulletSpawn.position, Quaternion.identity);
                Projectile projectile = b.GetComponent<Projectile>();
                if (projectile)
                {
                    projectile.owner = transform.root.gameObject;
                    projectile.damage = baseDamage;
                    if (Physics.Raycast(combatControls.player.camera.transform.position, combatControls.player.camera.transform.forward, out RaycastHit hit))
                    {
                        projectile.direction = (hit.point - bulletSpawn.position).normalized;
                    }
                    else
                    {
                        projectile.direction = combatControls.player.camera.transform.forward;
                    }
                }
                shootTimer = fireRate;
            }
        }
    }

    public void ChargeShot()
    {
        if(chargeTimer < maxChargeTime)
        {
            chargeTimer += Time.deltaTime;
        }


    }
}
