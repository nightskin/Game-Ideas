using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] LayerMask collisionMask;
    public GameObject owner = null;
    public Vector3 direction = Vector3.zero;
    public float speed = 20;
    public float range = 10;
    public float damage = 1;

    BoxCollider box;
    SphereCollider sphere;
    RaycastHit hit;

    void Start()
    {
        sphere = GetComponent<SphereCollider>();
        box = GetComponent<BoxCollider>();
    }
    
    void Update()
    {
        Vector3 prevPosition = transform.position;
        transform.position += direction * speed * Time.deltaTime;

        float distance = Vector3.Distance(transform.position, prevPosition);

        if (box)
        {
            if (Physics.BoxCast(prevPosition, box.size, direction, out hit, transform.rotation, distance, collisionMask))
            {
                CheckCollisions();
            }
        }
        else if (sphere)
        {
            if (Physics.SphereCast(prevPosition, sphere.radius, direction, out hit, distance, collisionMask))
            {
                CheckCollisions();
            }
        }
        else
        {
            if (Physics.Linecast(prevPosition, transform.position, out hit, collisionMask))
            {
                CheckCollisions();
            }
        }

        range -= Time.deltaTime;
        if (range <= 0)
        {
            Destroy(gameObject);
        }

    }

    void CheckCollisions()
    {
        if (hit.transform.gameObject != owner && !hit.collider.isTrigger)
        {
            // Apply damage to whatever here
            
            //
            Destroy(gameObject);
        }

    }

}
