using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoBehaviour
{
    public static GameObject player;
    public static Controls controls;
    public static PauseMenu pauseMenu;
    public static bool navigateUiWithMouse = false;

    //Game Settings
    public static bool slowCameraMovementWhenAttacking = true;
    public static bool slowCameraMovementWhenDefending = true;
    public static float aimSense = 100;


    
    void Awake()
    {
        player = GameObject.Find("Player");

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
    
    public void QuitToMenu()
    {

    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }

}
