using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject owner = null;
    public Vector3 direction = Vector3.zero;
    public float speed = 20;
    public float lifeTime = 10;

    BoxCollider box;

    void Start()
    {
        box = GetComponent<BoxCollider>();
    }
    
    void Update()
    {
        Vector3 prevPosition = transform.position;
        transform.position += direction * speed * Time.deltaTime;

        float distance = Vector3.Distance(transform.position, prevPosition);

        if (Physics.BoxCast(prevPosition, box.size / 2, direction, out RaycastHit hit, transform.rotation, distance))
        {
            if(hit.transform.gameObject != owner) 
            {
                Destroy(gameObject);
            }
        }

        lifeTime -= Time.deltaTime;
        if (lifeTime < 0) 
        {
            Destroy(gameObject);
        }
    }
}
