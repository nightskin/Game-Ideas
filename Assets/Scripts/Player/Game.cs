using UnityEngine;

public class Game : MonoBehaviour
{
    public static Controls controls;
    public static bool slowCameraMovementWhenAttacking = true;
    public static bool slowCameraMovementWhenDefending = true;
    public static bool cameraBob = true;
    public static float aimSensitivity = 100;
    void Awake()
    {
        controls = new Controls();
        controls.Enable();
    }
}
