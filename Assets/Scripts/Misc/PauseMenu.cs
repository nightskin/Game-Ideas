using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] List<Transform> stateNodes = new List<Transform>();
    int currentIndex = 0;

    void OnEnable()
    {
        for (int i = 0; i < stateNodes.Count; i++)
        {
            if (i == currentIndex) stateNodes[i].gameObject.SetActive(true);
            else stateNodes[i].gameObject.SetActive(false);
        }

        Game.controls.UI.ToggleMenuUp.performed += ToggleMenuUp_performed;
        Game.controls.UI.ToggleMenuDown.performed += ToggleMenuDown_performed;
    }
    
    void OnDisable()
    {
        Game.controls.UI.ToggleMenuUp.performed -= ToggleMenuUp_performed;
        Game.controls.UI.ToggleMenuDown.performed -= ToggleMenuDown_performed;
    }

    private void ToggleMenuDown_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        ToggleMenuDown();
    }

    private void ToggleMenuUp_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        ToggleMenuUp();
    }


    public void Open()
    {
        Time.timeScale = 0;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }

    public void ToggleMenuUp()
    {
        if(currentIndex > stateNodes.Count - 1)
        {
            currentIndex = 0;
        }
        else
        {
            currentIndex++;
        }

        for (int i = 0; i < stateNodes.Count; i++)
        {
            if(i == currentIndex) stateNodes[i].gameObject.SetActive(true);
            else stateNodes[i].gameObject.SetActive(false);
        }
    }

    public void ToggleMenuDown()
    {
        if (currentIndex < 1)
        {
            currentIndex = stateNodes.Count - 1;
        }
        else
        {
            currentIndex--;
        }

        for (int i = 0; i < stateNodes.Count; i++)
        {
            if (i == currentIndex) stateNodes[i].gameObject.SetActive(true);
            else stateNodes[i].gameObject.SetActive(false);
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
