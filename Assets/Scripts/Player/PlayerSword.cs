using UnityEngine;

public class PlayerSword: MonoBehaviour
{
    public ParticleSystem trail;

    [SerializeField] PlayerCombatControls combatControls;
    [SerializeField] GameObject impactEffectEnemy;
    [SerializeField] GameObject impactEffectSolid;
    [SerializeField] MeshRenderer meshRenderer;

    public bool slashing = false;
    public float knockbackForce = 10;
    public float damage = 1;

    void Start()
    {
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
                if (other.transform.root.GetComponent<EnemyHealth>())
                {
                    EnemyHealth health = other.transform.root.GetComponent<EnemyHealth>();
                    health.TakeDamage();
                    if(health.IsDead())
                    {
                        health.SetRagDoll(true);
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
