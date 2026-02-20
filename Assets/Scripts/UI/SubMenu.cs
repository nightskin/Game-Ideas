using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SubMenu : MonoBehaviour
{
    [SerializeField] EventSystem eventSystem;
    [SerializeField] GameObject firstSelected;
    void OnEnable()
    {
        if(Gamepad.current != null)
        {
            eventSystem.SetSelectedGameObject(firstSelected);
        }
    }
}
