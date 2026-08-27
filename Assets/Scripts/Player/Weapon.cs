using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] GameObject hitEffectPrefab;
    [SerializeField] LayerMask hitLayer;
    [SerializeField] BoxCollider collider;
    [SerializeField] GameObject trail;

    public bool isMagical;
    //public GameObject owner;
    public int damage = 1;


    public IEnumerator AnimateTrail()
    {
        float t = -1;
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
    
    void OnTriggerEnter(Collider other)
    {
        
    }

}
