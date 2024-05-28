using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileRandomizer : MonoBehaviour
{
    [SerializeField] int tileIndex = 0;
    [SerializeField] Sprite[] selectableTiles;
    [SerializeField] SpriteRenderer renderer;

    void Start()
    {
        if(!renderer) renderer = transform.Find("Mesh").GetComponent<SpriteRenderer>();

        tileIndex = Random.Range(0, selectableTiles.Length);
        renderer.sprite = selectableTiles[tileIndex];


    }
}
