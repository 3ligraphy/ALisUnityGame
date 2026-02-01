using UnityEngine;

public class ClosePopup : MonoBehaviour
{
    public GameObject popupUI;
    public FirstPersonController fps;

    public void Close()
    {
        popupUI.SetActive(false);
        
        // Show all icons again
        InfoIconManager.SetPopupOpen(false);
        
        if (fps != null)
        {
            fps.SetUIOpen(false);
        }
    }
}
