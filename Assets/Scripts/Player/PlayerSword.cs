using UnityEngine;
public class PlayerSword : MonoBehaviour
{
    [SerializeField] PlayerCombatControls combatControls;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] Transform bladeTip;
    [SerializeField] Transform bladeBase;
    [SerializeField] LayerMask hitLayer;
    public LineRenderer trail;

    [Min(1)] public int damage = 1;

    [HideInInspector] public bool trailOn;
    void RenderTrail()
    {
        if (trailOn)
        {
            for (int t = 0; t < trail.positionCount; t++)
            {
                trail.SetPosition(t, new Vector3(t, 0, 0));
            }
        }
    }

    void Start()
    {
        trailOn = false;
        trail.positionCount = 5;
        trail.useWorldSpace = false;
    }

    void Update()
    {
        if (combatControls.state == PlayerCombatControls.PlayerSwordState.ATK)
        {
            RenderTrail();
            if (Physics.Linecast(bladeBase.position, bladeTip.position, out RaycastHit rayHit, hitLayer))
            {
                if (rayHit.transform.tag == "Enemy")
                {
                    Debug.Log("You Scored A Hit");
                }
                else if (rayHit.transform.tag == "EnemyWeapon")
                {
                    Debug.Log("Your Attack Was Blocked");
                }
            }
        }
        else if (combatControls.state == PlayerCombatControls.PlayerSwordState.DEF)
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
