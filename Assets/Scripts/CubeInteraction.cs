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
    [Tooltip("Max time between two taps to count as double-tap (reduces accidental opens on touch)")]
    public float doubleTapMaxDelay = 0.4f;

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
        if (isPopupOpen) return;

        bool isTouch = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        bool isMouse = Input.GetMouseButtonDown(0);

        if (Application.isMobilePlatform || Input.touchCount > 0)
        {
            if (!isTouch) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;
        }
        else
        {
            if (!isMouse) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
        }

        CheckForClick();
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