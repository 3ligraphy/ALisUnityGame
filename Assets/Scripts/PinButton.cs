using UnityEngine;
using UnityEngine.UI;

public class PinButton : MonoBehaviour
{
    [Header("Pin Data")]
    public string placeName;               // اسم المكان
    [TextArea] public string placeInfo;    // وصف المكان
    public int sceneIndex;                 // رقم المشهد

    private Button button;
    private PopupManager popupManager;

    void Awake()
    {
        button = GetComponent<Button>();
        popupManager = FindObjectOfType<PopupManager>();
    }

    void OnEnable()
    {
        button.onClick.AddListener(OnPinClicked);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(OnPinClicked);
    }

    private void OnPinClicked()
    {
        if (popupManager != null)
        {
            popupManager.ShowPopup(placeName, placeInfo, sceneIndex);
        }
        else
        {
            Debug.LogError("PinButton: PopupManager not found in scene.");
        }
    }
}
