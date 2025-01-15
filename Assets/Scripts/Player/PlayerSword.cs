using UnityEngine;

public class PlayerSword: MonoBehaviour
{
    public ParticleSystem trail;

    [SerializeField] PlayerCombatControls combatControls;
    [SerializeField] GameObject impactEffectEnemy;
    [SerializeField] GameObject slashProjectile;
    [SerializeField] GameObject impactEffectSolid;
    [SerializeField] MeshRenderer meshRenderer;

    public float chargeValue = 0;
    public float maxChargeValue = 2;
    public bool slashing = false;
    public float knockbackForce = 10;
    public float damage = 1;

    public void ChargeWeapon()
    {
        chargeValue += Time.deltaTime;
    }

    public void ReleaseCharge()
    {
        if(chargeValue > maxChargeValue) 
        {
            var p = Instantiate(slashProjectile);
            p.transform.position = Camera.main.transform.position;
            p.transform.localEulerAngles = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, combatControls.atkAngle + 90);
            p.GetComponent<Projectile>().owner = transform.root.gameObject;
            p.GetComponent<Projectile>().direction = Camera.main.transform.forward;
        }
        chargeValue = 0;
    }

    void Start()
    {
        chargeValue = 0;
        if(!combatControls) combatControls = transform.root.GetComponent<PlayerCombatControls>();
        if(!meshRenderer) meshRenderer = transform.GetComponent<MeshRenderer>();
        if(transform.Find("Trail")) trail = transform.Find("Trail").GetComponent<ParticleSystem>();
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
