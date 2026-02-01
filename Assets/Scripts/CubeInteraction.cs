using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Simple cube interaction - click directly on the object from any distance.
/// </summary>
public class CubeInteraction : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupUI; // The popup panel
    public Button closeButton; // Optional close button
    
    [Header("Click Detection")]
    [Tooltip("Radius of the click detection sphere - larger = easier to click")]
    public float clickRadius = 2.0f;

    private FirstPersonController playerController;
    private Camera mainCamera;
    private bool isPopupOpen = false;

    void Start()
    {
        // Find player for SetUIOpen when popup opens
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<FirstPersonController>();
        }
        
        // Setup close button if assigned
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePopup);
        }
        
        // Ensure popup is hidden
        if (popupUI != null)
        {
            popupUI.SetActive(false);
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

    void ShowPopup()
    {
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (popupUI != null)
        {
            popupUI.SetActive(true);
        }
        
        isPopupOpen = true;
        
        if (playerController != null)
        {
            playerController.SetUIOpen(true);
        }
    }

    void HidePopup()
    {
        if (popupUI != null)
        {
            popupUI.SetActive(false);
        }
        
        isPopupOpen = false;
        
        if (playerController != null)
        {
            playerController.SetUIOpen(false);
        }
    }
}