using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    public Animator animator;
    void Start()
    {
        if(!animator) animator = GetComponent<Animator>();
    }
}
