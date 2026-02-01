using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles exhibit information display.
/// Click directly on the object from any distance to open popup.
/// </summary>
public class ExhibitInfo : MonoBehaviour
{
    [Header("Exhibit Data")]
    public string exhibitName;
    [TextArea] public string exhibitInfo;
    public Sprite exhibitSprite; // The exhibit image
    
    [Header("Click Detection")]
    [Tooltip("Radius of the click detection sphere - larger = easier to click")]
    public float clickRadius = 2.0f;

    private ExhibitPopupUI popupUI;
    private bool isPopupOpen = false;
    private FirstPersonController playerController;
    private Camera mainCamera;

    void Start()
    {
        popupUI = FindObjectOfType<ExhibitPopupUI>();
        
        if (popupUI == null)
        {
            Debug.LogError($"ExhibitInfo ({gameObject.name}): ExhibitPopupUI not found in scene!");
        }
        
        // Ensure this object has a collider for raycasting
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
        
        // Ensure EventSystem exists
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
        // Skip if popup is open
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
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Use SphereCast for larger click area (easier to click)
        if (Physics.SphereCast(ray, clickRadius, out hit, Mathf.Infinity))
        {
            // Check if we hit this object or any of its children
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                ShowPopup();
                return;
            }
        }
        
        // Also try regular raycast as backup for close objects
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                ShowPopup();
            }
        }
    }

    /// <summary>
    /// Shows the exhibit popup. Works from any distance.
    /// </summary>
    void ShowPopup()
    {
        if (popupUI == null)
        {
            Debug.LogWarning($"ExhibitInfo ({gameObject.name}): popupUI is null, cannot show popup");
            return;
        }
        
        // Show cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        popupUI.ShowPopup(exhibitName, exhibitInfo, exhibitSprite);
        isPopupOpen = true;
        
        // Notify player controller
        if (playerController == null)
        {
            playerController = FindObjectOfType<FirstPersonController>();
        }
        if (playerController != null)
        {
            playerController.SetUIOpen(true);
        }
    }

    /// <summary>
    /// Called when popup is closed (from ExhibitPopupUI)
    /// </summary>
    public void OnPopupClosed()
    {
        isPopupOpen = false;
        
        if (playerController != null)
        {
            playerController.SetUIOpen(false);
        }
    }
}
