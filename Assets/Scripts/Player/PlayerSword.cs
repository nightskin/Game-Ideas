using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSword: MonoBehaviour
{
    [SerializeField] Color trailColor = Color.white;
    [SerializeField] Color chargingColor = Color.cyan;
    [SerializeField] Color fullyChargedColor = Color.blue;
    public ParticleSystem trail;

    [SerializeField] PlayerCombatControls combatControls;
    [SerializeField] GameObject impactEffectEnemy;
    [SerializeField] GameObject impactEffectSolid;
    [SerializeField] GameObject slashEffect;
    [SerializeField] MeshRenderer meshRenderer;
    

    public float knockbackForce = 10;
    public float damage = 1;
 
    [SerializeField] float chargeTime = 2.5f;
    float chargeTimer;

    public bool FullyCharged()
    {
        return chargeTimer >= chargeTime;
    }

    public void ChargeWeapon()
    {
        chargeTimer += Time.deltaTime;
        if(FullyCharged())
        {
            meshRenderer.materials[1].color = fullyChargedColor;
        }
        else
        {
            meshRenderer.materials[1].color = Color.Lerp(meshRenderer.material.color, chargingColor, chargeTimer / chargeTime);
        }

    }
    
    public void ReleaseCharge()
    {
        Vector3 spawnPos = combatControls.player.camera.transform.position + combatControls.player.camera.transform.forward;
        GameObject powerSlash = Instantiate(slashEffect, spawnPos, Quaternion.identity);

        float zRot = combatControls.armPivot.localEulerAngles.z - 90;
        powerSlash.transform.eulerAngles = Quaternion.LookRotation(combatControls.player.camera.transform.forward).eulerAngles + new Vector3(0, 0, zRot);
        powerSlash.GetComponent<MeshRenderer>().material.color = fullyChargedColor;
        powerSlash.GetComponent<Projectile>().owner = gameObject;
        powerSlash.GetComponent<Projectile>().direction = combatControls.player.camera.transform.forward;

        LoseCharge();
    }

    public void LoseCharge()
    {
        chargeTimer = 0;
        meshRenderer.materials[1].color = Color.white;
    }

    void Start()
    {
        LoseCharge();
        if(!combatControls) combatControls = transform.root.GetComponent<PlayerCombatControls>();
        if(!meshRenderer) meshRenderer = transform.GetComponent<MeshRenderer>();
        if(transform.Find("Trail")) trail = transform.Find("Trail").GetComponent<ParticleSystem>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (combatControls.slashing)
        {
            if (other.transform.tag == "Solid" || other.tag == "EnemyWeapon")
            {
                if(impactEffectSolid) Instantiate(impactEffectSolid, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
                combatControls.animator.SetTrigger("recoil");
                combatControls.slashing = false;
            }
            else if (other.transform.tag == "CanBeDamaged")
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
