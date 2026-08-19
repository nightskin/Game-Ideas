using UnityEngine;

public class SetPosition : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    //public Vector3 rotation;

    void Start()
    {
        transform.position = target.position + offset;
    }

    void OnValidate()
    {
        transform.position = target.position + offset;
    }

}
