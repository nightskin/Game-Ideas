using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] Slider AimSenseSlider;
    [SerializeField] Slider slowCamAtk;
    [SerializeField] Slider slowCamDef;
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
        Game.controls.UI.SwitchToGamepad.performed += SwitchToGamepad_performed;
        Game.controls.UI.SwitchToMouse.performed += SwitchToMouse_performed;
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

    private void SwitchToMouse_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Game.navigateUiWithMouse = true;
    }

    private void SwitchToGamepad_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Game.navigateUiWithMouse = false;
    }

    public void Open()
    {
        Time.timeScale = 0;
        gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ToggleMenuUp()
    {
        if(currentIndex >= stateNodes.Count - 1)
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

    public void ChangeAimSense()
    {
        Game.aimSense = AimSenseSlider.value * 10;
        Game.player.GetComponent<Player>().lookSpeed = Game.aimSense;
    }

    public void ChangeSlowCamAtk()
    {
        Game.slowCameraAtkAmount = slowCamAtk.value;
    }

    public void ChangeSlowCamDef()
    {
        Game.slowCameraDefAmont = slowCamDef.value;
    }

}
