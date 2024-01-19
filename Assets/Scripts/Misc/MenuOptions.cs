using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuOptions : MonoBehaviour
{
    [SerializeField] PlayerHUD hud;

    public void UnPause()
    {
        Cursor.lockState = CursorLockMode.Locked;
        hud.pauseMenu.SetActive(false);
        Time.timeScale = 1;
        hud.paused = false;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Level");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BackToMenu()
    {
        if(hud) UnPause();
        SceneManager.LoadScene("MainMenu");
    }

}
