using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject owner = null;
    public Vector3 direction = Vector3.zero;
    public float speed = 20;
    


    void Start()
    {
        
    }


    void Update()
    {
        if(Vector3.Distance(owner.transform.position, transform.position) <= 1000)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject != owner) 
        {
            Destroy(gameObject);
        }
    }

}
