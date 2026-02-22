using System.Collections;
using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    [SerializeField] PlayerCombatControls player;
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] LayerMask hitLayer;
    [SerializeField] BoxCollider collider;
    public GameObject trail;
    public bool fullyCharged = false;
    public bool magical;

    float t = -1;

    public int power = 1;

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
        if (!trail) trail = transform.GetChild(0).gameObject;
        if (!player) player = transform.root.GetComponent<PlayerCombatControls>();
        if (!collider) collider = GetComponent<BoxCollider>();

    }



}
