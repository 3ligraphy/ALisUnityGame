using UnityEngine;
using UnityEngine.Events;

public class CubeInteraction : MonoBehaviour
{
    public GameObject popupUI; // اسحب الـ Panel هنا

    void OnMouseDown()
    {
        // لازم يكون الماوس ظاهر
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // إظهار الـ Popup
        if (popupUI != null)
        {
            popupUI.SetActive(true);
        }
    }
}