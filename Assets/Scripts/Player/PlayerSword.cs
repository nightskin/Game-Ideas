using UnityEngine;

public class PlayerMeleeWeapon: MonoBehaviour
{

    public ParticleSystem trail;
    public ParticleSystem chargeEffect;
    public  Material glowMaterial;

    [SerializeField] Color unChargedColor = Color.cyan;
    [SerializeField] Color chargedColor = new Color(1,0.5f,0);
    [SerializeField] PlayerCombatControls combatControls;
    [SerializeField] GameObject impactEffectEnemy;
    [SerializeField] GameObject slashProjectile;
    [SerializeField] GameObject impactEffectSolid;
    [SerializeField] MeshRenderer meshRenderer;

    public bool canCharge = true;
    float chargeTime = 0;
    public float chargeValue = 0;
    public float maxChargeValue = 2;
    public bool slashing = false;
    public float knockbackForce = 10;
    public float damage = 1;

    public void ChargeWeapon()
    {
        if(canCharge)
        {
            if(!chargeEffect.gameObject.activeSelf)
            {
                chargeEffect.gameObject.SetActive(true);
            }
            chargeTime += Time.deltaTime;
            chargeValue = Mathf.Lerp(0, maxChargeValue, chargeTime);
            glowMaterial.SetColor("_EmissionColor", Color.Lerp(unChargedColor * 8, chargedColor * 8, chargeTime));
        }
        else
        {
            chargeTime = 0;
            chargeValue = 0;
            chargeEffect.gameObject.SetActive(false);
            glowMaterial.SetColor("_EmissionColor", unChargedColor * 8);
        }
    }

    public void ReleaseCharge()
    {
        if(chargeValue >= maxChargeValue) 
        {
            var p = Instantiate(slashProjectile);
            p.transform.position = Camera.main.transform.position;
            p.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, transform.root.localEulerAngles.y , combatControls.atkAngle + 90);
            p.GetComponent<Projectile>().owner = transform.root.gameObject;
            p.GetComponent<Projectile>().direction = Camera.main.transform.forward;
        }
        chargeTime = 0;
        chargeValue = 0;
        chargeEffect.gameObject.SetActive(false);
        glowMaterial.SetColor("_EmissionColor", unChargedColor * 8);
    }

    void OnTriggerEnter(Collider other)
    {
        if (slashing)
        {
            if (other.transform.tag == "Solid" || other.tag == "EnemyWeapon")
            {
                if(impactEffectSolid) Instantiate(impactEffectSolid, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                combatControls.animator.SetTrigger("recoil");
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

                if (other.attachedRigidbody)
                {
                    Vector3 forceDirection = (transform.root.right * combatControls.atkVector.x + transform.root.up * combatControls.atkVector.y);
                    other.attachedRigidbody.AddForce(forceDirection * knockbackForce);
                }

            }
        }
    }
}
