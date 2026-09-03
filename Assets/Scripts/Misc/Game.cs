using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Game : MonoBehaviour
{
    public static Game get;
    public static Input input;
    public static GameSettings settings;

    
    public PauseMenu pauseMenu;
    public GameObject player;
    public NoiseGPU noise;


    
    void Awake()
    {
        get = this;
        settings = new GameSettings();
        input = new Input();
        input.Enable();
    }
    
    void OnEnable()
    {
        input.Player.Pause.performed += Pause_performed;
    }

    void OnDisable()
    {
        input.Player.Pause.performed -= Pause_performed;
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
