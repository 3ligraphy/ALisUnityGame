using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Ultra-lightweight info icon system.
/// Uses a SINGLE static manager to update ALL icons - no per-icon Update() overhead.
/// Icons are simple UI Images without TextMeshPro for maximum performance.
/// </summary>
public class InfoIconManager : MonoBehaviour
{
    [Header("Icon Settings")]
    public float iconHeight = 3.0f;
    public float iconSize = 80f; // Default size - can be overridden per-scene
    public Color iconBackgroundColor = new Color(0.2f, 0.6f, 1f, 1f);
    
    [Header("Performance")]
    [Tooltip("Max distance to show icon. 0 = always visible (no distance limit)")]
    public float maxVisibleDistance = 0f; // 0 = always visible

    // Static manager handles ALL icons from one place
    private static IconUpdateManager updateManager;
    private static bool anyPopupOpen = false;
    private static float lastClickTime = 0f;
    private const float CLICK_COOLDOWN = 0.3f;

    // Instance data
    private GameObject iconUI;
    private RectTransform iconRect;
    private Transform targetTransform;
    private System.Action onIconClicked;
    private bool isRegistered = false;
    private bool isVisible = false;

    public static void SetPopupOpen(bool isOpen)
    {
        anyPopupOpen = isOpen;
        if (updateManager != null)
        {
            updateManager.SetAllIconsActive(!isOpen);
        }
    }

    public void Initialize(Transform target, System.Action onClick)
    {
        targetTransform = target;
        onIconClicked = onClick;
        
        EnsureUpdateManager();
        CreateIconUI();
        // Don't hide initially - let ShowIcon control visibility
    }

    static void EnsureUpdateManager()
    {
        if (updateManager != null) return;
        
        GameObject managerObj = new GameObject("InfoIconUpdateManager");
        updateManager = managerObj.AddComponent<IconUpdateManager>();
        Object.DontDestroyOnLoad(managerObj);
    }

    void CreateIconUI()
    {
        EnsureUpdateManager();
        
        // Create simple icon - just an Image with "i" as text overlay
        iconUI = new GameObject($"Icon_{targetTransform.name}");
        iconUI.transform.SetParent(updateManager.GetCanvas().transform, false);
        
        iconRect = iconUI.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        
        // Background circle
        Image bgImage = iconUI.AddComponent<Image>();
        bgImage.color = iconBackgroundColor;
        
        // Simple "i" using a child Text (lighter than TMP)
        GameObject textObj = new GameObject("i");
        textObj.transform.SetParent(iconUI.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Use Unity's built-in Text instead of TextMeshPro (much lighter)
        Text iText = textObj.AddComponent<Text>();
        iText.text = "i";
        iText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        iText.fontSize = Mathf.RoundToInt(iconSize * 0.5f);
        iText.fontStyle = FontStyle.BoldAndItalic;
        iText.color = Color.white;
        iText.alignment = TextAnchor.MiddleCenter;
        iText.raycastTarget = false;
        
        // Button for click
        Button btn = iconUI.AddComponent<Button>();
        btn.targetGraphic = bgImage;
        btn.onClick.AddListener(OnIconClick);
        
        iconUI.SetActive(false);
    }

    public void ShowIcon()
    {
        isVisible = true;
        if (!isRegistered && updateManager != null)
        {
            updateManager.RegisterIcon(this);
            isRegistered = true;
        }
        // Immediately try to show icon if camera is available
        if (iconUI != null && Camera.main != null)
        {
            UpdateIconPosition(Camera.main);
        }
    }

    public void HideIcon()
    {
        isVisible = false;
        if (iconUI != null)
        {
            iconUI.SetActive(false);
        }
    }

    void OnIconClick()
    {
        if (Time.time - lastClickTime < CLICK_COOLDOWN) return;
        lastClickTime = Time.time;
        
        // Safety check before invoking
        if (onIconClicked != null)
        {
            try
            {
                onIconClicked.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"InfoIconManager: Error in click handler: {e.Message}");
            }
        }
    }

    public bool IsIconVisible()
    {
        return isVisible && iconUI != null && iconUI.activeSelf;
    }

    // Called by the central manager - not per-icon Update()
    public void UpdateIconPosition(Camera cam)
    {
        // Safety checks
        if (cam == null || iconUI == null || targetTransform == null)
        {
            return;
        }
        
        if (!isVisible || anyPopupOpen)
        {
            if (iconUI.activeSelf)
                iconUI.SetActive(false);
            return;
        }

        // No distance culling when maxVisibleDistance is 0 (always visible)
        if (maxVisibleDistance > 0)
        {
            float distance = Vector3.Distance(cam.transform.position, targetTransform.position);
            if (distance > maxVisibleDistance)
            {
                if (iconUI.activeSelf) iconUI.SetActive(false);
                return;
            }
        }

        Vector3 worldPos = targetTransform.position + Vector3.up * iconHeight;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // Behind camera check
        if (screenPos.z < 0)
        {
            if (iconUI.activeSelf) iconUI.SetActive(false);
            return;
        }

        // On screen check
        if (screenPos.x < 0 || screenPos.x > Screen.width || 
            screenPos.y < 0 || screenPos.y > Screen.height)
        {
            if (iconUI.activeSelf) iconUI.SetActive(false);
            return;
        }

        // Show and position
        if (!iconUI.activeSelf) iconUI.SetActive(true);
        iconRect.position = screenPos;
    }

    void OnDestroy()
    {
        if (updateManager != null && isRegistered)
        {
            updateManager.UnregisterIcon(this);
        }
        if (iconUI != null)
        {
            Destroy(iconUI);
        }
    }
}

/// <summary>
/// Central manager that updates ALL icons from a single Update() call.
/// Massively reduces overhead compared to per-icon MonoBehaviour.Update().
/// </summary>
public class IconUpdateManager : MonoBehaviour
{
    private Canvas canvas;
    private Camera mainCamera;
    private List<InfoIconManager> icons = new List<InfoIconManager>();

    void Awake()
    {
        CreateCanvas();
    }

    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("InfoIconCanvas");
        canvasObj.transform.SetParent(transform);
        
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        
        // No CanvasScaler - use raw pixels for predictable sizing
        canvasObj.AddComponent<GraphicRaycaster>();
    }

    public Canvas GetCanvas() => canvas;

    public void RegisterIcon(InfoIconManager icon)
    {
        if (!icons.Contains(icon))
            icons.Add(icon);
    }

    public void UnregisterIcon(InfoIconManager icon)
    {
        icons.Remove(icon);
    }

    public void SetAllIconsActive(bool active)
    {
        if (canvas != null)
            canvas.gameObject.SetActive(active);
    }

    void Update()
    {
        if (icons.Count == 0) return;
        
        // Try to get camera every frame in case it changes
        mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Update ALL icons every frame to ensure they appear immediately
        for (int i = icons.Count - 1; i >= 0; i--)
        {
            if (i < icons.Count && icons[i] != null)
            {
                icons[i].UpdateIconPosition(mainCamera);
            }
        }
    }
}
