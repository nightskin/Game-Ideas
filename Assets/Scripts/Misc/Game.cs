using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoBehaviour
{
    public GameObject player;
    public static Controls controls;
    public static PauseMenu pauseMenu;
    public static bool slowCameraMovementWhenAttacking = true;
    public static bool slowCameraMovementWhenDefending = true;
    public static float mouseSensitivity = 100;
    
    void Awake()
    {
        if (!player) player = GameObject.Find("Player");

        controls = new Controls();
        controls.Enable();
        pauseMenu = transform.Find("Canvas").GetChild(0).GetComponent<PauseMenu>();

        controls.Player.Pause.performed += Pause_performed;

    }

    void OnDestroy()
    {
        controls.Player.Pause.performed -= Pause_performed;
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        if (pauseMenu.gameObject.activeSelf)
        {
            pauseMenu.Close();
        }
        else
        {
            pauseMenu.Open();
        }
    }

}
