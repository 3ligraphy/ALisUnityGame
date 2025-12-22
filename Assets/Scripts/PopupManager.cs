using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PopupManager : MonoBehaviour
{
    [Header("Popup Elements")]
    public GameObject popupPanel;      // البانل بتاع البوب أب
    public TMP_Text placeNameText;     // اسم المكان (TMP)
    public TMP_Text placeInfoText;     // وصف المكان (TMP)
    public Button visitButton;         // زرار Visit (لسه Button عادي)

    private int targetSceneIndex = -1; // رقم المشهد المطلوب

    void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);

        if (visitButton != null)
        {
            visitButton.onClick.RemoveAllListeners();
            visitButton.onClick.AddListener(OnVisitClicked);
        }
    }

    public void ShowPopup(string placeName, string placeInfo, int sceneIndex)
    {
        targetSceneIndex = sceneIndex;

        if (placeNameText != null) placeNameText.text = placeName;
        if (placeInfoText != null) placeInfoText.text = placeInfo;

        if (popupPanel != null) popupPanel.SetActive(true);
    }

    public void ClosePopup()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
        targetSceneIndex = -1;
    }

    private void OnVisitClicked()
    {
        if (targetSceneIndex >= 0)
        {
            SceneManager.LoadScene(targetSceneIndex);
        }
        else
        {
            Debug.LogWarning("PopupManager: No target scene set.");
        }
    }
}
