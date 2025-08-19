using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public enum EnemyCombatState
    {
        IDLE,
        ATK,
        DEF,
    }
    public EnemyCombatState combatState;
    [SerializeField] Transform bladeBase;
    [SerializeField] Transform bladeTip;

    [SerializeField] LayerMask hitLayerMask;

    void Update()
    {
        if (combatState == EnemyCombatState.ATK)
        {
            if (Physics.Linecast(bladeBase.position, bladeTip.position, out RaycastHit hitInfo, hitLayerMask))
            {
                if (hitInfo.transform.tag == "PlayerWeapon")
                {
                    combatState = EnemyCombatState.IDLE;
                }
                else if (hitInfo.transform.tag == "Player")
                {
                    hitInfo.transform.GetComponent<HealthScript>().TakeDamage(1);
                    combatState = EnemyCombatState.IDLE;
                }
            }
        }
        else if (combatState == EnemyCombatState.DEF)
        {
            if (Physics.Linecast(bladeBase.position, bladeTip.position, out RaycastHit hitInfo, hitLayerMask))
            {
                
            }
        }
    }
}
