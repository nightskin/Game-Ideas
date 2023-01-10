using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySwarmAI : MonoBehaviour
{
    Rigidbody rb;

    public Transform target;
    public float moveSpeed = 10;
    public float atkRange = 1;
    public float jumpHeight = 3;
    
    bool attacked = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        target = GameObject.Find("Player").transform;
    }

    void Update()
    {
        Vector3[] points =
        {
            Cohesion(5),
            GetTargetRot(),
        };

        Vector3 targetRot = points[0] + points[1] / points.Length;
        transform.forward = Vector3.Lerp(transform.forward, targetRot, moveSpeed * Time.deltaTime);
        rb.position += (transform.forward * moveSpeed * Time.deltaTime);


        if (Vector3.Distance(transform.position, target.position) <= atkRange)
        {
            JumpAttack();
        }

    }
    
    void JumpAttack()
    {
        if(!attacked)
        {
            rb.position += Vector3.up * Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y) * Time.deltaTime;
            attacked = true;
        }
        attacked = false;
    }

    Vector3 Cohesion(float radius)
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, radius, transform.forward);
        Vector3 pos = Vector3.one;
        if(hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                pos += hits[i].transform.position;
            }
            pos /= hits.Length;

        }
        return new Vector3(pos.x, 0, pos.z) - new Vector3(transform.position.x, 0, transform.position.z);
    }

    Vector3 GetTargetRot()
    {
       return new Vector3(target.position.x, 0, target.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
    }
}
