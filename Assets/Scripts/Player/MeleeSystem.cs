using UnityEngine;

public class MeleeSystem : MonoBehaviour
{
    [SerializeField] Transform armPivot;
    [SerializeField] Transform weapon;

    [SerializeField][Range(0, 1)] float actionDamp = 0.5f;
    
    FirstPersonPlayer player;
    public Animator animator;


    float atkAngle;
    Vector2 atkVector;
    float defaultLookSpeed;
    public Transform lockOnTarget = null;


    void Start()
    {
        atkAngle = 0;
        atkVector = Vector2.zero;
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
        defaultLookSpeed = player.lookSpeed;

        player.actions.Slash.performed += Slash_performed;
        player.actions.Slash.canceled += Slash_canceled;
        player.actions.LockOn.performed += LockOn_performed;
        player.actions.LockOn.canceled += LockOn_canceled;
    }

    void Update()
    {
        atkVector = player.actions.Look.ReadValue<Vector2>();
        if (atkVector.magnitude > 0.5f)
        {
            atkAngle = Mathf.Atan2(atkVector.x, -atkVector.y) * 180 / Mathf.PI;
        }
    }

    void OnDestroy()
    {
        player.actions.Slash.performed -= Slash_performed;
        player.actions.Slash.canceled -= Slash_canceled;
        player.actions.LockOn.performed -= LockOn_performed;
        player.actions.LockOn.canceled -= LockOn_canceled;
    }

    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetTrigger("atk");
    }

    private void Slash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    private void LockOn_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(Physics.Raycast(player.camera.position, player.camera.forward, out RaycastHit hit))
        {
            if(hit.transform.gameObject.layer == 6)
            {
                lockOnTarget = hit.transform;
            }
        }
    }

    private void LockOn_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        lockOnTarget = null;
    }



    ///Animation Events
    public void StartAttack()
    {
        weapon.GetComponent<PlayerWeapon>().attacking = true;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
        //player.lookSpeed *= actionDamp;
    }
    
    public void EndAttack()
    {
        weapon.GetComponent<PlayerWeapon>().attacking = false;
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        //player.lookSpeed = defaultLookSpeed;
    }

}
