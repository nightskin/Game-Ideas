using UnityEngine;
using UnityEngine.VFX;
public class PlayerSword : MonoBehaviour
{
    [SerializeField] PlayerCombatControls combatControls;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] Transform bladeTip;
    [SerializeField] Transform bladeBase;
    [SerializeField] LayerMask hitLayer;
    [SerializeField] Material bladeMaterialForCharging;
    public LineRenderer trail;

    [SerializeField] bool isMagic;
    Color normalColor;
    Color chargeColor = new Color(0, 2, 2);
    float charge;
    [SerializeField] float maxCharge = 1;
    [Min(0)] public int power = 1;

    public void ChargeWeapon()
    {
        if (charge < maxCharge)
        {
            charge += Time.deltaTime;
            float v = charge;
            bladeMaterialForCharging.SetColor("_Tint2", Color.Lerp(normalColor, chargeColor, v));
        }
        else
        {
            charge = maxCharge;
        }
    }

    public bool IsSwordMagical()
    {
        return isMagic;
    }

    public bool IsFullyCharged()
    {
        return charge >= maxCharge;
    }

    public void ResetCharge()
    {
        if (isMagic)
        {
            charge = 0;
            bladeMaterialForCharging.SetColor("_Tint2", normalColor);
        }
    }

    void RenderTrail()
    {
        if(trail)
        {
            if (trail.gameObject.activeSelf)
            {
                for (int t = 0; t < trail.positionCount; t++)
                {
                    trail.SetPosition(t, new Vector3(t, 0, 0));
                }
            }
        }
    }

    void Start()
    {
        charge = 0;
        normalColor = bladeMaterialForCharging.GetColor("_Tint2");
        if (!combatControls) combatControls = transform.root.GetComponent<PlayerCombatControls>();
        if (trail)
        {
            trail.startWidth = Vector3.Distance(bladeBase.position, bladeTip.position);
            trail.endWidth = 0;
            trail.positionCount = 5;
            trail.useWorldSpace = false;
            trail.gameObject.SetActive(false);
        }
        if (!bladeTip) bladeTip = transform.Find("Tip");
        if (!bladeBase) bladeBase = transform.Find("Base");
    }

    void Update()
    {
        if (combatControls.state == PlayerCombatControls.PlayerCombatState.ATK)
        {
            RenderTrail();
            if (Physics.Linecast(bladeBase.position, bladeTip.position, out RaycastHit rayHit, hitLayer))
            {
                if (rayHit.transform.tag == "Enemy")
                {
                    var fx = Instantiate(hitEffectPrefab, rayHit.point, Quaternion.identity);
                }
                else if (rayHit.transform.tag == "EnemyWeapon" || rayHit.transform.tag == "Solid")
                {
                    var fx = Instantiate(hitEffectPrefab, rayHit.point, Quaternion.identity);
                    fx.GetComponent<VisualEffect>().SetVector4("Color", new Vector4(4, 4, 4, 1));
                    combatControls.state = PlayerCombatControls.PlayerCombatState.IDLE;
                }
            }
        }
    }
}
