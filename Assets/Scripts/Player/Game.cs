using UnityEngine;

public class Game : MonoBehaviour
{
    public static Controls controls;
    public static bool slowCameraMovementWhenAttacking = true;
    public static bool slowCameraMovementWhenDefending = true;
    public static bool cameraBob = false;
    public static float mouseSensitivity = 100;
    void Awake()
    {
        controls = new Controls();
        controls.Enable();
    }
}
