using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Handles interaction with clickable objects (like wells).
/// Click directly on the object from any distance to open popup.
/// </summary>
public class CubeClickHandler : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel; // The popup UI panel
    public Button closeButton;    // Close button
    
    [Header("Click Detection")]
    [Tooltip("Radius of the click detection sphere - larger = easier to click")]
    public float clickRadius = 2.0f;
    [Tooltip("Max time between two taps to count as double-tap (reduces accidental opens on touch)")]
    public float doubleTapMaxDelay = 0.4f;

    // Private references
    private FirstPersonController playerController;
    private Camera mainCamera;
    private bool isPopupOpen = false;
    private float lastTapTime = -999f;
    private Transform lastTappedTransform = null;

    void Start()
    {
        // Find player for SetUIOpen when popup opens
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<FirstPersonController>();
        }

        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePopup);
        }

        // Ensure popup is hidden initially
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
        
        // Ensure this object has a collider for raycasting
        if (GetComponent<Collider>() == null)
        {
            // Add a box collider if none exists
            gameObject.AddComponent<BoxCollider>();
        }
        
        // Ensure EventSystem exists for UI clicks
        EnsureEventSystem();
    }
    
    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }
    }

    void Update()
    {
        // Skip if popup is open or clicking on UI
        if (isPopupOpen) return;
        
        // Check for click/tap
        if (Input.GetMouseButtonDown(0))
        {
            // Don't process if clicking on UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
            
            CheckForClick();
        }
        
        // Also check for touch on mobile
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;
                
            CheckForClick();
        }
    }
    
    void CheckForClick()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Transform hitTransform = InteractionHitCache.GetHitTransformForTap(mainCamera, clickRadius);
        bool hitThis = hitTransform != null && (hitTransform == transform || hitTransform.IsChildOf(transform));

        if (hitThis)
        {
            if (lastTappedTransform == transform && (Time.time - lastTapTime) <= doubleTapMaxDelay)
            {
                lastTapTime = -999f;
                lastTappedTransform = null;
                ShowPopup();
            }
            else
            {
                lastTapTime = Time.time;
                lastTappedTransform = transform;
            }
        }
        else
        {
            lastTappedTransform = null;
        }
    }

    /// <summary>
    /// Shows the popup and notifies player controller
    /// </summary>
    void ShowPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
        
        isPopupOpen = true;
        
        // Show cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Notify player controller that UI is open
        if (playerController != null)
        {
            playerController.SetUIOpen(true);
        }
    }

    /// <summary>
    /// Hides the popup and notifies player controller
    /// </summary>
    void HidePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
        
        isPopupOpen = false;
        
        // Notify player controller that UI is closed
        if (playerController != null)
        {
            playerController.SetUIOpen(false);
        }
    }
}