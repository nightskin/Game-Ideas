using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] LayerMask hitLayer;
    [SerializeField] BoxCollider collider;
    [SerializeField] GameObject trail;

    float t;
    
    public enum WeaponSize
    {
        SMALL,
        MEDIUM,
        LARGE,
    }
    public WeaponSize size;
    public bool isMagical;
    public GameObject owner;
    public int damage = 1;


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

    public bool HasTrail()
    {
        if(trail != null)
        {
            return true;
        }
        return false;
    }

    void Start()
    {
        if (!trail) trail = transform.GetChild(0).gameObject;
        if (!owner)
        {
            owner = transform.root.gameObject;
        }
        if (!collider) collider = GetComponent<BoxCollider>();

    }
}
