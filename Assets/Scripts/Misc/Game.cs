using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoBehaviour
{
    public PauseMenu pauseMenu;
    public GameObject player;

    public static Controls controls;
    public static bool navigateUiWithMouse = false;

    //Game Settings
    public static float cameraBob = 1;
    public static float slowCameraAtkAmount = 0.1f;
    public static float slowCameraDefAmont = 0.1f;
    public static float aimSense = 100;
    
    void Awake()
    {
        controls = new Controls();
        controls.Enable();
    }
    
    void OnEnable()
    {
        controls.Player.Pause.performed += Pause_performed;
    }

    void OnDisable()
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
