using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Handles fullscreen video display for cinema scene.
/// Click on the video screen from any distance to go fullscreen.
/// Use the minimize button to return to normal view.
/// </summary>
public class VideoFullscreenController : MonoBehaviour
{
    [Header("Video References")]
    [Tooltip("The VideoPlayer component (optional - will auto-find)")]
    public VideoPlayer videoPlayer;
    [Tooltip("The RenderTexture used by the video (optional - will auto-find)")]
    public RenderTexture videoRenderTexture;
    
    [Header("Click Detection")]
    [Tooltip("Radius of the click detection sphere - larger = easier to click")]
    public float clickRadius = 2.0f;
    [Tooltip("Max time between two taps to count as double-tap (reduces accidental fullscreen on touch)")]
    public float doubleTapMaxDelay = 0.4f;
    
    [Header("Fullscreen UI Settings")]
    [Tooltip("Background color for fullscreen mode")]
    public Color fullscreenBackgroundColor = Color.black;
    [Tooltip("Size of minimize button")]
    public float minimizeButtonSize = 60f;
    [Tooltip("Margin from screen edge for minimize button")]
    public float minimizeButtonMargin = 20f;
    
    // UI elements created at runtime
    private GameObject fullscreenCanvas;
    private GameObject fullscreenPanel;
    private RawImage fullscreenVideoImage;
    private GameObject minimizeButton;
    private bool isFullscreen = false;
    
    // References
    private Camera mainCamera;
    private FirstPersonController playerController;
    private float lastTapTime = -999f;
    private Transform lastTappedTransform = null;
    
    void Start()
    {
        // Find video player if not assigned
        if (videoPlayer == null)
        {
            videoPlayer = FindObjectOfType<VideoPlayer>();
        }
        
        // Get render texture from video player
        if (videoPlayer != null && videoRenderTexture == null)
        {
            videoRenderTexture = videoPlayer.targetTexture;
        }
        
        // Ensure this object has a collider for raycasting
        if (GetComponent<Collider>() == null)
        {
            // Add a box collider sized to the renderer bounds
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                col.size = rend.bounds.size;
                col.center = transform.InverseTransformPoint(rend.bounds.center);
            }
        }
        
        // Find player controller
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<FirstPersonController>();
        }
        
        // Ensure EventSystem exists
        EnsureEventSystem();
        
        // Create fullscreen UI (hidden initially)
        CreateFullscreenUI();
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
        if (isFullscreen) return;

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
                EnterFullscreen();
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
    /// Creates the fullscreen UI overlay
    /// </summary>
    void CreateFullscreenUI()
    {
        // Create canvas for fullscreen view
        fullscreenCanvas = new GameObject("VideoFullscreenCanvas");
        Canvas canvas = fullscreenCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // On top of everything
        
        CanvasScaler scaler = fullscreenCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        fullscreenCanvas.AddComponent<GraphicRaycaster>();
        
        // Create black background panel
        fullscreenPanel = new GameObject("FullscreenPanel");
        fullscreenPanel.transform.SetParent(fullscreenCanvas.transform, false);
        
        RectTransform panelRect = fullscreenPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = fullscreenPanel.AddComponent<Image>();
        panelImage.color = fullscreenBackgroundColor;
        
        // Create video display (RawImage for RenderTexture)
        GameObject videoObj = new GameObject("FullscreenVideo");
        videoObj.transform.SetParent(fullscreenPanel.transform, false);
        
        RectTransform videoRect = videoObj.AddComponent<RectTransform>();
        videoRect.anchorMin = Vector2.zero;
        videoRect.anchorMax = Vector2.one;
        videoRect.offsetMin = Vector2.zero;
        videoRect.offsetMax = Vector2.zero;
        
        fullscreenVideoImage = videoObj.AddComponent<RawImage>();
        if (videoRenderTexture != null)
        {
            fullscreenVideoImage.texture = videoRenderTexture;
        }
        
        // Create minimize button (top-right corner)
        CreateMinimizeButton();
        
        // Hide fullscreen UI initially
        fullscreenCanvas.SetActive(false);
    }
    
    /// <summary>
    /// Creates the minimize button
    /// </summary>
    void CreateMinimizeButton()
    {
        minimizeButton = new GameObject("MinimizeButton");
        minimizeButton.transform.SetParent(fullscreenPanel.transform, false);
        
        // Position in top-right corner
        RectTransform btnRect = minimizeButton.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.sizeDelta = new Vector2(minimizeButtonSize, minimizeButtonSize);
        btnRect.anchoredPosition = new Vector2(-minimizeButtonMargin, -minimizeButtonMargin);
        
        // Button background
        Image btnImage = minimizeButton.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // Button component
        Button btn = minimizeButton.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(ExitFullscreen);
        
        // X icon text
        GameObject iconObj = new GameObject("MinimizeIcon");
        iconObj.transform.SetParent(minimizeButton.transform, false);
        
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = "✕";
        iconText.fontSize = 32;
        iconText.fontStyle = FontStyles.Bold;
        iconText.color = Color.white;
        iconText.alignment = TextAlignmentOptions.Center;
        
        // Add label below icon
        GameObject labelObj = new GameObject("MinimizeLabel");
        labelObj.transform.SetParent(minimizeButton.transform, false);
        
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, -0.5f);
        labelRect.anchorMax = new Vector2(1, 0);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = "MINIMIZE";
        labelText.fontSize = 10;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.Center;
    }
    
    /// <summary>
    /// Enter fullscreen mode
    /// </summary>
    public void EnterFullscreen()
    {
        if (isFullscreen) return;
        
        isFullscreen = true;
        
        // Update render texture reference in case it changed
        if (videoPlayer != null && videoRenderTexture == null)
        {
            videoRenderTexture = videoPlayer.targetTexture;
        }
        
        if (fullscreenVideoImage != null && videoRenderTexture != null)
        {
            fullscreenVideoImage.texture = videoRenderTexture;
        }
        
        // Show fullscreen UI
        if (fullscreenCanvas != null)
        {
            fullscreenCanvas.SetActive(true);
        }
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Notify player controller
        if (playerController != null)
        {
            playerController.SetUIOpen(true);
        }
        
        // Play video if paused
        if (videoPlayer != null && !videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
        
        Debug.Log("VideoFullscreenController: Entered fullscreen mode");
    }
    
    /// <summary>
    /// Exit fullscreen mode
    /// </summary>
    public void ExitFullscreen()
    {
        if (!isFullscreen) return;
        
        isFullscreen = false;
        
        // Hide fullscreen UI
        if (fullscreenCanvas != null)
        {
            fullscreenCanvas.SetActive(false);
        }
        
        // Notify player controller
        if (playerController != null)
        {
            playerController.SetUIOpen(false);
        }
        
        Debug.Log("VideoFullscreenController: Exited fullscreen mode");
    }
    
    /// <summary>
    /// Check if currently in fullscreen mode
    /// </summary>
    public bool IsFullscreen()
    {
        return isFullscreen;
    }
    
    void OnDestroy()
    {
        if (fullscreenCanvas != null)
        {
            Destroy(fullscreenCanvas);
        }
    }
}
