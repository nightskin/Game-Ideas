using UnityEngine;

public class PlayerGun : MonoBehaviour
{
    [SerializeField] GameObject normalBulletPrefab;
    [SerializeField] GameObject chargeBulletPrafeb;
    [SerializeField] PlayerCombatControls combatControls;
    [SerializeField] Transform bulletSpawn;

    public float baseDamage = 1;
    public bool shooting = false;
   
    public enum ShootStyle
    {
        RAPID_FIRE,
        CHARGE_BLAST
    }
    public ShootStyle shootStyle;


    [Header("Rapid Fire Settings")]
    public float fireRate = 0.1f;
    float shootTimer = 0;

    [Header("Charge Blast Settings")]
    public float maxChargeTime = 1;
    float chargeTimer = 0;
    Projectile currentChargeShot = null;

    void Start()
    {
        if(!combatControls) combatControls = transform.root.GetComponent<PlayerCombatControls>();
        if(!bulletSpawn) bulletSpawn = transform.GetChild(0);
    }
    
    public void Fire()
    {
        if(shootStyle == ShootStyle.RAPID_FIRE)
        {
            var b = Instantiate(normalBulletPrefab, bulletSpawn.position, Quaternion.identity);
            Projectile projectile = b.GetComponent<Projectile>();
            if (projectile)
            {
                projectile.owner = transform.root.gameObject;
                projectile.damage = baseDamage;
                if (combatControls.reticle.color == Color.red)
                {
                    projectile.direction = (combatControls.lockOnHit.point - bulletSpawn.position).normalized;
                }
                else
                {
                    projectile.direction = combatControls.player.camera.transform.forward;
                }
            }
            shootTimer = fireRate;
        }
        else if(shootStyle == ShootStyle.CHARGE_BLAST)
        {
            currentChargeShot = Instantiate(chargeBulletPrafeb, bulletSpawn.position, Quaternion.identity).GetComponent<Projectile>();
            if (currentChargeShot)
            {
                currentChargeShot.GetComponent<TrailRenderer>().enabled = false;
                currentChargeShot.owner = transform.root.gameObject;
                currentChargeShot.damage = baseDamage;
            }
        }
    }

    public void RapidFire()
    {
        if(shootStyle == ShootStyle.RAPID_FIRE)
        {
            if (shootTimer > 0)
            {
                shootTimer -= Time.deltaTime;
            }
            else
            {
                var b = Instantiate(normalBulletPrefab, bulletSpawn.position, Quaternion.identity);
                Projectile projectile = b.GetComponent<Projectile>();
                if (projectile)
                {
                    projectile.owner = transform.root.gameObject;
                    projectile.damage = baseDamage;

                    if (combatControls.reticle.color == Color.red)
                    {
                        projectile.direction = (combatControls.lockOnHit.point - bulletSpawn.position).normalized;
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

    public void Charge()
    {
        if (currentChargeShot)
        {
            currentChargeShot.transform.position = bulletSpawn.position;

            if (chargeTimer < maxChargeTime)
            {
                chargeTimer += Time.deltaTime;
                currentChargeShot.damage += Time.deltaTime;
                currentChargeShot.transform.localScale += Vector3.one * Time.deltaTime;
            }
        }
    }
    
    public void ReleaseCharge()
    {
        if (currentChargeShot)
        {
            currentChargeShot.GetComponent<TrailRenderer>().enabled = true;
            currentChargeShot.GetComponent<TrailRenderer>().startWidth = currentChargeShot.transform.localScale.x;

            if(combatControls.reticle.color == Color.red)
            {
                currentChargeShot.direction = (combatControls.lockOnHit.point - bulletSpawn.position).normalized;
            }
            else
            {
                currentChargeShot.direction = combatControls.player.camera.transform.forward;
            }
            currentChargeShot.released = true;
            chargeTimer = 0;
            currentChargeShot = null;
        }

    }
}
