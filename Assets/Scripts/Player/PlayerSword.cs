using System.Collections;
using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    [SerializeField] PlayerCombatControls player;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] LayerMask hitLayer;
    [SerializeField] GameObject chargeEffectObj;
    [SerializeField] Material chargeEffectMat;
    [SerializeField] BoxCollider collider;
    public GameObject trail;

    [SerializeField] bool magical;

    [SerializeField] Color startChargeColor = Color.white * 2;
    [SerializeField] Color endChargeColor = Color.cyan * 2;
    Color chargeColor = Color.white * 2;
    float charge = 0;
    float t = -1;
    [SerializeField] float maxCharge = 1;
    [Min(0)] public int power = 1;

    public void ChargeWeapon()
    {
        player.animator.SetTrigger("charging");
        if (!chargeEffectObj.activeSelf) chargeEffectObj.SetActive(true);

        if (charge < maxCharge)
        {
            charge += Time.deltaTime;
        }
        else
        {
            charge = maxCharge;
        }

        chargeColor = Color.Lerp(startChargeColor, endChargeColor, charge);
        chargeEffectMat.SetColor("_Color", chargeColor);

    }

    public bool IsSwordMagical()
    {
        return magical;
    }

    public bool IsFullyCharged()
    {
        return charge >= maxCharge;
    }

    public void ResetCharge()
    {
        if (magical)
        {
            charge = 0;
            chargeColor = startChargeColor;
            chargeEffectMat.SetColor("_Color", startChargeColor);
            chargeEffectObj.SetActive(false);
            player.animator.SetBool("charging", false);
        }
    }
    
    public IEnumerator AnimateTrail()
    {
        t = -1;
        trail.GetComponent<MeshRenderer>().material.SetFloat("_Scroll", t);
        trail.SetActive(true);

        while(trail.GetComponent<MeshRenderer>().material.GetFloat("_Scroll") < 1)
        {
            t += (2 / 0.15f) * Time.deltaTime;
            trail.GetComponent<MeshRenderer>().material.SetFloat("_Scroll", t);
            yield return null;
        }

        trail.SetActive(false);
    }

    void Start()
    {
        charge = 0;
        if (!trail) trail = transform.GetChild(0).gameObject;
        if (!player) player = transform.root.GetComponent<PlayerCombatControls>();
        if (!collider) collider = GetComponent<BoxCollider>();

    }



}
