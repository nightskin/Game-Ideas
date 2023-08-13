using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeSystem : MonoBehaviour
{
    public Transform armPivot;
    public Transform weapon;

    private FirstPersonPlayer player;
    public Animator animator;


    public enum AxisType
    {
        LOOK,
        MOVE,
    }
    public AxisType actionAxisType;
    private Vector2 defaultLookSpeed;
    [SerializeField] [Range(0, 1)] private float atkDamp = 0.1f;

    private Vector2 actionAxis = new Vector2();
    public float atkAngle = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<FirstPersonPlayer>();
        defaultLookSpeed = player.lookSpeed;

        player.actions.Slash.performed += Slash_performed;
        player.actions.Slash.canceled += Slash_canceled;
    }
    
    void Update()
    {
        if (actionAxisType == AxisType.MOVE) actionAxis = player.actions.Move.ReadValue<Vector2>();
        else if (actionAxisType == AxisType.LOOK) actionAxis = player.actions.Look.ReadValue<Vector2>();

        if (animator.GetBool("slash"))
        {
            if (actionAxis.magnitude > 0)
            {
                atkAngle = Mathf.Atan2(actionAxis.x, -actionAxis.y) * (180 / Mathf.PI);
            }
        }
    }

    void OnDestroy()
    {
        player.actions.Slash.performed -= Slash_performed;
        player.actions.Slash.canceled -= Slash_canceled;
    }

    private void Slash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetBool("slash", true);
        player.lookSpeed *= atkDamp;
    }

    private void Slash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.SetBool("slash", false);
    }

    ///Animation Events
    public void StartAttack()
    {
        weapon.GetComponent<PlayerWeapon>().attacking = true;
        if (actionAxisType == AxisType.LOOK) player.lookSpeed *= atkDamp;
        armPivot.localEulerAngles = new Vector3(0, 0, atkAngle);
    }
    
    public void EndAttack()
    {
        armPivot.localEulerAngles = new Vector3(0, 0, 0);
        weapon.GetComponent<PlayerWeapon>().attacking = false;
        player.lookSpeed = defaultLookSpeed;
    }

}
