using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the exhibit popup UI display.
/// Includes a close button that notifies ExhibitInfo when popup is closed.
/// </summary>
public class ExhibitPopupUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public TMP_Text exhibitNameText;
    public TMP_Text exhibitInfoText;
    public Image exhibitImage;
    public Button closeButton; // Optional close button

    // Event for when popup is closed (so ExhibitInfo can show icon again)
    public System.Action OnPopupClosed;
    
    // Reference to the ExhibitInfo that opened this popup
    private ExhibitInfo currentExhibit;

    void Start()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
        
        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    /// <summary>
    /// Shows the popup with exhibit information
    /// </summary>
    public void ShowPopup(string name, string info, Sprite image)
    {
        if (exhibitNameText != null)
        {
            exhibitNameText.text = name;
        }
        
        if (exhibitInfoText != null)
        {
            exhibitInfoText.text = info;
        }

        if (exhibitImage != null)
        {
            if (image != null)
            {
                exhibitImage.sprite = image;
                exhibitImage.gameObject.SetActive(true);
            }
            else
            {
                exhibitImage.gameObject.SetActive(false);
            }
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the popup
    /// </summary>
    public void HidePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
        
        // Notify listeners that popup was closed
        OnPopupClosed?.Invoke();
    }

    /// <summary>
    /// Called when close button is clicked
    /// </summary>
    void OnCloseButtonClicked()
    {
        HidePopup();
        
        // Notify player controller
        FirstPersonController playerController = FindObjectOfType<FirstPersonController>();
        if (playerController != null)
        {
            playerController.SetUIOpen(false);
        }
    }

    /// <summary>
    /// Check if popup is currently visible
    /// </summary>
    public bool IsPopupVisible()
    {
        return popupPanel != null && popupPanel.activeInHierarchy;
    }
}
