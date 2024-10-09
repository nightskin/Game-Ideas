using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject owner = null;
    public Vector3 direction = Vector3.zero;
    public float speed = 20;
    public float lifeTime = 10;
    public int maxNumberOfBounces;
    public float damage = 1;

    int bounces = 0;
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

        if(box)
        {
            if (Physics.BoxCast(prevPosition, box.size / 2, direction, out hit, transform.rotation, distance))
            {
                CheckCollisions();
            }
        }
        else if (sphere)
        {
            if(Physics.SphereCast(prevPosition, sphere.radius, direction ,out hit, distance))
            {
                CheckCollisions();
            }
        }
        else
        {
            if(Physics.Raycast(prevPosition, direction, out hit, distance))
            {
                CheckCollisions();
            }
        }

        lifeTime -= Time.deltaTime;
        if (lifeTime < 0) 
        {
            Destroy(gameObject);
        }
    }

    void CheckCollisions()
    {
        if (hit.transform.gameObject != owner)
        {
            bounces++;
            if(bounces > maxNumberOfBounces)
            {
                Destroy(gameObject);
            }
            else
            {
                direction = Vector3.Reflect(direction, hit.normal);
            }



        }
    }
}
