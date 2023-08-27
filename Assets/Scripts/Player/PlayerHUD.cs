using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] FirstPersonPlayer player;
    public Image hitOverlay;
    public GameObject pauseMenu;
    public bool paused;

    float flickerTime = 0.05f;
    bool flicker;
    bool fadeToBlack;


    void Start()
    {
        paused = false;
        flicker = false;
        fadeToBlack = false;
        player.actions.Pause.performed += Pause_performed;
    }

    void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(paused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
            paused = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
            pauseMenu.SetActive(true);
            paused = true;
        }
    }

    void Update()
    {
        if(flicker && !fadeToBlack)
        {
            flickerTime -= Time.deltaTime;
            if(flickerTime <= 0)
            {
                hitOverlay.color = Color.clear;
                flickerTime = 0.05f;
                flicker = false;
            }
            else
            {
                hitOverlay.color = Color.red;
            }
        }
        if(fadeToBlack)
        {
            hitOverlay.color = Color.Lerp(hitOverlay.color, Color.black, 10 * Time.deltaTime);
            if(hitOverlay.color == Color.black)
            {
                SceneManager.LoadScene("GameOver");
            }
        }
    }

    public void Flicker()
    {
        flicker = true;
    }
    public void FadeToBlack()
    {
        fadeToBlack = true;
    }
}
