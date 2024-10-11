using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] float pauseTime = 0;
    [SerializeField] float moveSpeed = 10;
    int index = 0;
    float pauseTimer = 0;



    [Header("FOR DEBUG")]
    [SerializeField] List<Vector3> points = new List<Vector3>();
    [SerializeField] Color pointColor = Color.yellow;


    
    void OnValidate()
    {
        if(points.Count == 0)
        {
            points.Add(transform.position);
        }
    }

    void OnDrawGizmos()
    {
        if(points.Count > 0)
        {
            for(int i = 0;  i < points.Count; i++) 
            {
                Gizmos.color = pointColor;
                Gizmos.DrawSphere(points[i], 1);
            }
        }
    }

    void FixedUpdate()
    {
        Vector3 curentPoint = points[index];
        if(transform.position != curentPoint)
        {
            transform.position = Vector3.MoveTowards(transform.position, curentPoint, moveSpeed * Time.deltaTime);
        }
        else
        {
            if(Countdown())
            {
                IncrementIndex();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            other.transform.SetParent(null);
        }
    }

    void IncrementIndex()
    {
        if(index < points.Count - 1)
        {
            index++;
        }
        else
        {
            index = 0;
        }
    }

    bool Countdown()
    {
        if(pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            return false;
        }
        else
        {
            pauseTimer = pauseTime;
            return true;
        }
    }

}
