using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject owner = null;
    public Vector3 direction = Vector3.zero;
    public float speed = 20;
    public float lifeTime = 10;

    BoxCollider box;
    SphereCollider sphere;

    RaycastHit hit;

    void Start()
    {
        sphere = GetComponent<SphereCollider>();
        box = GetComponent<BoxCollider>();
    }
    
    void FixedUpdate()
    {
        Vector3 prevPosition = transform.position;
        transform.position += direction * speed * Time.fixedDeltaTime;

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
            Destroy(gameObject);
        }
    }
}
