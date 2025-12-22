using UnityEngine;

public class ClosePopup : MonoBehaviour
{
    public GameObject popupUI;
    public FirstPersonController fps;

    public void Close()
    {
        popupUI.SetActive(false);
        fps.SetUIOpen(false); // رجّع الحركة والكاميرا
    }
}
