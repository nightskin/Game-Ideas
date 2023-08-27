using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    [SerializeField] DungeonGen level;
    [SerializeField] Transform player;
    void Start()
    {
        transform.position = new Vector3(level.tilesX/2 * level.tileSize, 10 , level.tilesZ/2 * level.tileSize);
    }

    void LateUpdate()
    {
        Vector3 newpos = player.position;
        newpos.y = transform.position.y;
        transform.position = newpos;
    }

}
