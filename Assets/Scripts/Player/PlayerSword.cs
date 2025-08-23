using UnityEngine;
using UnityEngine.VFX;
public class PlayerSword : MonoBehaviour
{
    [SerializeField] PlayerCombatControls combatControls;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] Transform bladeTip;
    [SerializeField] Transform bladeBase;
    [SerializeField] LayerMask hitLayer;
    public LineRenderer trail;

    [Min(1)] public int damage = 1;

    void RenderTrail()
    {
        if (trail.gameObject.activeSelf)
        {
            for (int t = 0; t < trail.positionCount; t++)
            {
                trail.SetPosition(t, new Vector3(t, 0, 0));
            }
        }
    }

    void Start()
    {
        if (trail)
        {
            trail.startWidth = Vector3.Distance(bladeBase.position, bladeTip.position);
            trail.endWidth = 0;
            trail.positionCount = 5;
            trail.useWorldSpace = false;
            trail.gameObject.SetActive(false);
        }

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
        else if (combatControls.state == PlayerCombatControls.PlayerCombatState.DEF)
        {
            if (Physics.Linecast(bladeBase.position, bladeTip.position, out RaycastHit rayHit, hitLayer))
            {
                if (rayHit.transform.tag == "EnemyWeapon")
                {

                }
            }
        }

    }
}
