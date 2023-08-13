using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaker : MonoBehaviour
{
    public static bool armed = false;
    bool prevValue;
    [SerializeField] GameObject armedPlayer;
    [SerializeField] GameObject unarmedPlayer;

    void Start()
    {
        prevValue = armed;
        if (armed) Instantiate(armedPlayer, transform.position, Quaternion.identity);
        else Instantiate(unarmedPlayer, transform.position, Quaternion.identity);
    }

    private void Update()
    {
        if(prevValue != armed)
        {
            if (armed)
            {
                Destroy(GameObject.FindGameObjectWithTag("Player"));
                Instantiate(armedPlayer, transform.position, Quaternion.identity);
            }
            else
            {
                Destroy(GameObject.FindGameObjectWithTag("Player"));
                Instantiate(unarmedPlayer, transform.position, Quaternion.identity);
            }
        }
        prevValue = armed;
    }

}
