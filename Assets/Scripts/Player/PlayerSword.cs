using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    [SerializeField] PlayerCombatControls player;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] Transform bladeTip;
    [SerializeField] Transform bladeBase;
    [SerializeField] LayerMask hitlayer;

    public float knockbackForce = 10;
    public float damage = 1;

    void Update()
    {
        if (player.state == PlayerCombatControls.PlayerSwordState.ATK)
        {
            if (Physics.Linecast(bladeBase.position, bladeTip.position, out RaycastHit rayHit, hitlayer))
            {
                if (rayHit.transform.tag == "Enemy")
                {
                    Instantiate(hitEffectPrefab, rayHit.point, Quaternion.identity);
                }
            }
        }
        else if (player.state == PlayerCombatControls.PlayerSwordState.DEF)
        {

        }
    }

}
