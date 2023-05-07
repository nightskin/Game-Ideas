using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TemporaryEffect : MonoBehaviour
{
    
    
    void Update()
    {
        if(!GetComponent<VisualEffect>().HasAnySystemAwake())
        {
            Destroy(gameObject);
        }
    }
}
