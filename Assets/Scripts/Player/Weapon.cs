using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] LayerMask hitLayer;
    [SerializeField] BoxCollider collider;
    [SerializeField] GameObject trail;

    public bool isMagical;
    public int damage = 1;

    void Start()
    {
        if(!player) player = transform.root.GetComponent<Player>();
        if(!collider) collider = transform.GetComponent<BoxCollider>();
    }

    void Update()
    {
        if(trail)
        {
            if(player.isAttacking)
            {
                trail.SetActive(true);
            }
            else
            {
                trail.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(player.isAttacking)
        {
            
        }
    }

}
