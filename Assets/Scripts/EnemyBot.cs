using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBot : MonoBehaviour
{
    public Transform target;

    [SerializeField] private Transform HeadLook;
    [SerializeField] private Transform WeaponLook;


    void Start()
    {
        
    }

    
    void Update()
    {
        FollowPlayer();
    }

    void FollowPlayer()
    {
        HeadLook.transform.position = Vector3.Lerp(HeadLook.position, target.position, 10 * Time.deltaTime);

    }
}
