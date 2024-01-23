using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float distanceFromplayer = 10;
    void Start()
    {
        transform.position = player.position + (Vector3.up * distanceFromplayer);
    }

    void LateUpdate()
    {
        transform.position = player.position + (Vector3.up * distanceFromplayer);
    }

}
