using UnityEngine;

public class ChunkingBehaviour : MonoBehaviour
{
    public string playerName = "Warrior";
    public float maxDistance = 500;
    GameObject player;

    void Start()
    {
        player = GameObject.Find(playerName);
    }

    void FixedUpdate()
    {
        if(!player) return;

        if(Vector3.Distance(player.transform.position, transform.position) > maxDistance)
        {
            if(transform.GetChild(0).gameObject.activeSelf) transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            if(!transform.GetChild(0).gameObject.activeSelf) transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}
