using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupSceneLoader : MonoBehaviour
{
    [Header("Popup Settings")]
    public GameObject popupPanel;       // نافذة البوب أب
    public int targetSceneIndex = 1;    // رقم المشهد اللي هينتقل له (تحدد من الـ Inspector)

    [Header("UI Buttons")]
    public Button openPopupButton;      // الزر اللي يفتح النافذة
    public Button startButton;          // زر Start داخل النافذة
    public Button closeButton;          // زر غلق النافذة

    void Start()
    {
        // في البداية النافذة تكون مخفية
        if (popupPanel != null)
            popupPanel.SetActive(false);

        // ربط الأحداث بالأزرار
        if (openPopupButton != null)
            openPopupButton.onClick.AddListener(OpenPopup);

        if (startButton != null)
            startButton.onClick.AddListener(LoadTargetScene);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePopup);
    }

    void OpenPopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(true);
    }

    void ClosePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    void LoadTargetScene()
    {
        SceneManager.LoadScene(targetSceneIndex);
    }
}
