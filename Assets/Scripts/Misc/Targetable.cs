using UnityEngine;

public class Targetable : MonoBehaviour
{   
    

    void OnBecameVisible()
    {
        Game.get.targets.Add(transform);
    }

    void OnBecameInvisible()
    {
        Game.get.targets.Remove(transform);        
    }

}
