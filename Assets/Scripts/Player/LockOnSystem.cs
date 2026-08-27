using UnityEngine;

public class LockOnSystem : MonoBehaviour
{
    public Player core;
    public float maxDistance = 50;
    public LayerMask lockOnLayerMask;
    [HideInInspector] public Transform target = null;
    float lockOnLerp = 0;

    void Start()
    {
        if(!core) core = GetComponent<Player>();
    }

    void Update()
    {
        
    }

    void LookTowardsTarget()
    {
        Vector3 lookDirection = target.transform.position - core.cameraHolder.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);

        
        if(lockOnLerp < 1) lockOnLerp += Time.deltaTime;
        lockOnLerp = Mathf.Clamp01(lockOnLerp);

        core.rotx = Mathf.LerpAngle(core.rotx, lookRotation.eulerAngles.x, lockOnLerp);
        core.roty = Mathf.LerpAngle(core.roty, lookRotation.eulerAngles.y, lockOnLerp);

        core.cameraHolder.transform.localEulerAngles = new Vector3(core.rotx,0,0);
        transform.localEulerAngles = new Vector3(0,core.roty,0);


        if(Vector3.Distance(transform.position, target.position) > maxDistance)
        {
            lockOnLerp = 0;
            target = null;
        }
    }
    public void LockOn()
    {
        Ray ray = new Ray(core.cameraHolder.position, core.cameraHolder.forward);
        RaycastHit[] hits =  Physics.RaycastAll(ray, maxDistance, lockOnLayerMask);
        if(hits.Length == 0) return;
        Transform closest = hits[0].transform;
        
        foreach(RaycastHit hit in hits)
        {
            //If current iteration > closest
            float currentDot  = Vector3.Dot((hit.transform.position - core.cameraHolder.position).normalized, core.cameraHolder.forward);
            float closestDot = Vector3.Dot((closest.position - core.cameraHolder.position).normalized,core.cameraHolder.forward);
            if (currentDot > closestDot)
            {
                closest = hit.transform;
            }
        }
        
        target = closest;
    }
    public void LockOff()
    {

        target = null;
        lockOnLerp = 0;
    }
}
