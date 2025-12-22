using UnityEngine;
using UnityEngine.UI;

public class PopupCloseButton : MonoBehaviour
{
    private Button button;
    private PopupManager popupManager;

    void Awake()
    {
        button = GetComponent<Button>();
        popupManager = FindObjectOfType<PopupManager>();
    }

    void OnEnable()
    {
        button.onClick.AddListener(ClosePopup);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(ClosePopup);
    }

    void ClosePopup()
    {
        if (popupManager != null) popupManager.ClosePopup();
    }
}
