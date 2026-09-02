using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class LockOnSystem : PlayerAbility
{
    public float maxDistance = 50;
    public LayerMask lockOnLayerMask;
    [HideInInspector] public Transform target = null;
    float lockOnLerp = 0;

    public override void Init()
    {
        
    }

    public override void FixedUpdate()
    {
        
    }

    public override void Update()
    {
        if(Gamepad.current.rightStick.IsActuated())
        {
            if(target)
            {
                target = null;
                lockOnLerp = 0;
            }
            else
            {
                Ray ray = new Ray(owner.cameraHolder.position, owner.cameraHolder.forward);
                RaycastHit[] hits =  Physics.RaycastAll(ray, maxDistance, lockOnLayerMask);
                if(hits.Length == 0) return;
                Transform closest = hits[0].transform;

                foreach(RaycastHit hit in hits)
                {
                    float currentDot  = Vector3.Dot((hit.transform.position - owner.cameraHolder.position).normalized, owner.cameraHolder.forward);
                    float closestDot = Vector3.Dot((closest.position - owner.cameraHolder.position).normalized,owner.cameraHolder.forward);
                    if (currentDot > closestDot)
                    {
                        closest = hit.transform;
                    }
            }
        
            target = closest;
            }
        }

        if(target)
        {
            Vector3 lookDirection = target.transform.position - owner.cameraHolder.transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);

        
            if(lockOnLerp < 1) lockOnLerp += Time.deltaTime;
            lockOnLerp = Mathf.Clamp01(lockOnLerp);

            owner.rotx = Mathf.LerpAngle(owner.rotx, lookRotation.eulerAngles.x, lockOnLerp);
            owner.roty = Mathf.LerpAngle(owner.roty, lookRotation.eulerAngles.y, lockOnLerp);

            owner.cameraHolder.transform.localEulerAngles = new Vector3(owner.rotx,0,0);
            owner.transform.localEulerAngles = new Vector3(0,owner.roty,0);


            if(Vector3.Distance(owner.transform.position, target.position) > maxDistance)
            {
                lockOnLerp = 0;
                target = null;
            }
        }
    }

}
