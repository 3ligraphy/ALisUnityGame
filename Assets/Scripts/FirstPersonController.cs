using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 4.0f; // Increased jump height

    [Header("Touch Look Settings")]
    [Tooltip("Sensitivity for touch-based camera rotation")]
    public float touchSensitivity = 0.15f;
    [Tooltip("Mouse sensitivity for desktop testing")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    [Header("Joystick (Optional)")]
    public VariableJoystick joystick;

    [Header("UI Control")]
    public bool uiOpen = false;

    [Header("Touch Zones")]
    [Tooltip("The left portion of the screen reserved for joystick (0.0-1.0). Kept narrow so 'i' icons on the left are not captured as joystick.")]
    public float joystickZoneWidth = 0.22f;
    [Tooltip("The bottom portion of screen for joystick (0.0-1.0). Kept low so 'i' icons above are not captured.")]
    public float joystickZoneHeight = 0.35f;

    [Header("Jump Button Settings")]
    [Tooltip("Size of the jump button in pixels")]
    public float jumpButtonSize = 80f;
    [Tooltip("Margin from screen edges in pixels")]
    public float jumpButtonMargin = 30f;
    [Tooltip("Color of the jump button")]
    public Color jumpButtonColor = new Color(0.3f, 0.6f, 0.9f, 0.85f);
    [Tooltip("Color when jump button is pressed")]
    public Color jumpButtonPressedColor = new Color(0.5f, 0.8f, 1f, 1f);

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    
    // Touch tracking for multi-touch support
    private int lookTouchId = -1;  // ID of the finger used for camera look
    private Vector2 lastLookTouchPosition;
    
    // Track if we've been initialized in this scene
    private bool hasInitializedThisScene = false;
    
    // Jump button UI
    private GameObject jumpButtonObj;
    private Button jumpButton;
    private bool jumpButtonPressed = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Cursor visible on mobile
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Enable multi-touch
        Input.multiTouchEnabled = true;
        
        // Reset camera rotation state for new scene
        xRotation = 0f;
        
        // Find joystick
        FindJoystick();
        
        // Create jump button UI
        CreateJumpButton();
        
        hasInitializedThisScene = true;
        Debug.Log($"FirstPersonController: Started. Forward direction: {transform.forward}");
    }

    /// <summary>
    /// Called when the object becomes enabled. Handles scene transitions.
    /// </summary>
    void OnEnable()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Reset state when enabled (handles scene transitions)
        lookTouchId = -1;
        uiOpen = false;
        
        // If we haven't initialized yet, Start() will handle it
        if (!hasInitializedThisScene)
            return;
        
        // Re-initialize for new scene
        Debug.Log("FirstPersonController: OnEnable - Re-initializing for scene");
        
        // Re-find joystick (old one might be destroyed)
        FindJoystick();
        
        // Reset camera pitch
        xRotation = 0f;
        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    /// <summary>
    /// Called when disabled or destroyed
    /// </summary>
    void OnDisable()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Called when a new scene is loaded
    /// </summary>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"FirstPersonController: Scene loaded: {scene.name}");
        
        // Reset all state for the new scene
        lookTouchId = -1;
        uiOpen = false;
        xRotation = 0f;
        velocity = Vector3.zero;
        
        // Reset camera rotation
        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        
        // Re-find joystick for new scene
        // Delay by one frame to ensure scene is fully loaded
        StartCoroutine(ReinitializeAfterSceneLoad());
    }

    /// <summary>
    /// Coroutine to reinitialize after scene load (waits for scene to be fully ready)
    /// </summary>
    System.Collections.IEnumerator ReinitializeAfterSceneLoad()
    {
        yield return null; // Wait one frame
        
        // Find joystick in new scene
        FindJoystick();
        
        // Ensure jump button exists
        EnsureJumpButton();
        
        // Log player orientation for debugging
        Debug.Log($"FirstPersonController: After scene load - Forward: {transform.forward}, Rotation: {transform.rotation.eulerAngles}");
    }

    /// <summary>
    /// Finds the joystick in the scene and shrinks its click area so 'i' icons on the left are not captured.
    /// </summary>
    void FindJoystick()
    {
        joystick = FindObjectOfType<VariableJoystick>();
        if (joystick != null)
        {
            Debug.Log("FirstPersonController: Found VariableJoystick");
            ShrinkJoystickClickArea();
        }
        else
        {
            Debug.LogWarning("FirstPersonController: No VariableJoystick found in scene!");
        }
    }
    
    /// <summary>
    /// Shrinks the joystick's root RectTransform so its click area matches our zone (bottom-left only).
    /// Prevents the joystick from capturing touches meant for 'i' icons on the left side.
    /// </summary>
    void ShrinkJoystickClickArea()
    {
        if (joystick == null) return;
        
        RectTransform joystickRoot = joystick.GetComponent<RectTransform>();
        if (joystickRoot == null) return;
        
        // Anchor to bottom-left and size to exactly our zone (0-1 in anchor space)
        joystickRoot.anchorMin = new Vector2(0, 0);
        joystickRoot.anchorMax = new Vector2(joystickZoneWidth, joystickZoneHeight);
        joystickRoot.pivot = new Vector2(0, 0);
        joystickRoot.anchoredPosition = Vector2.zero;
        joystickRoot.sizeDelta = Vector2.zero;
        
        Debug.Log($"FirstPersonController: Joystick click area set to {joystickZoneWidth * 100}% width x {joystickZoneHeight * 100}% height");
    }

    /// <summary>
    /// Creates a jump button on the right side of the screen for mobile/touch devices.
    /// Uses a dedicated overlay canvas so the button always appears in bottom-right.
    /// </summary>
    void CreateJumpButton()
    {
        // If we have a valid button already, just ensure it's visible
        if (jumpButtonObj != null)
        {
            if (!jumpButtonObj.activeInHierarchy)
            {
                jumpButtonObj.SetActive(true);
            }
            return;
        }
        
        // Always use a dedicated canvas for the jump button (not the scene canvas)
        // so it persists and stays in correct screen position
        GameObject canvasObj = GameObject.Find("FirstPersonJumpButtonCanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("FirstPersonJumpButtonCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500; // On top of most UI
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100;
            canvasObj.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasObj);
        }
        
        Transform canvasTransform = canvasObj.transform;
        
        // Check if there's already a JumpButton (e.g. from previous scene load)
        Transform existingButton = canvasTransform.Find("JumpButton");
        if (existingButton != null)
        {
            jumpButtonObj = existingButton.gameObject;
            jumpButtonObj.SetActive(true);
            jumpButton = jumpButtonObj.GetComponent<Button>();
            SetupJumpButtonEvents();
            EnsureJumpButtonPosition();
            return;
        }
        
        // Create the jump button
        jumpButtonObj = new GameObject("JumpButton");
        jumpButtonObj.transform.SetParent(canvasTransform, false);
        
        // Set up RectTransform - bottom right corner (works with overlay)
        RectTransform buttonRect = jumpButtonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 0);  // Bottom-right anchor
        buttonRect.anchorMax = new Vector2(1, 0);
        buttonRect.pivot = new Vector2(1, 0);
        buttonRect.sizeDelta = new Vector2(jumpButtonSize, jumpButtonSize);
        buttonRect.anchoredPosition = new Vector2(-jumpButtonMargin, jumpButtonMargin);
        
        // Add background image (circular look)
        Image buttonImage = jumpButtonObj.AddComponent<Image>();
        buttonImage.color = jumpButtonColor;
        
        // Add Button component
        jumpButton = jumpButtonObj.AddComponent<Button>();
        ColorBlock colors = jumpButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        jumpButton.colors = colors;
        
        // Create arrow icon (upward arrow using text)
        GameObject iconObj = new GameObject("JumpIcon");
        iconObj.transform.SetParent(jumpButtonObj.transform, false);
        
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        
        TMP_Text iconText = iconObj.AddComponent<TextMeshProUGUI>();
        iconText.text = "↑";  // Upward arrow symbol
        iconText.fontSize = 36;
        iconText.fontStyle = FontStyles.Bold;
        iconText.color = Color.white;
        iconText.alignment = TextAlignmentOptions.Center;
        
        // Add small label below the arrow
        GameObject labelObj = new GameObject("JumpLabel");
        labelObj.transform.SetParent(jumpButtonObj.transform, false);
        
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 0.35f);
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        
        TMP_Text labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = "JUMP";
        labelText.fontSize = 12;
        labelText.fontStyle = FontStyles.Bold;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.Center;
        
        // Setup button events
        SetupJumpButtonEvents();
        
        Debug.Log("FirstPersonController: Jump button created (bottom-right)");
    }
    
    /// <summary>
    /// Ensures jump button is in the correct bottom-right position (handles canvas scale).
    /// </summary>
    void EnsureJumpButtonPosition()
    {
        if (jumpButtonObj == null) return;
        
        RectTransform rect = jumpButtonObj.GetComponent<RectTransform>();
        if (rect == null) return;
        
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.sizeDelta = new Vector2(jumpButtonSize, jumpButtonSize);
        rect.anchoredPosition = new Vector2(-jumpButtonMargin, jumpButtonMargin);
    }
    
    /// <summary>
    /// Sets up the event listeners for the jump button (press and release)
    /// </summary>
    void SetupJumpButtonEvents()
    {
        if (jumpButton == null) return;
        
        // Remove any existing listeners
        jumpButton.onClick.RemoveAllListeners();
        
        // Add EventTrigger for press/release detection
        EventTrigger trigger = jumpButtonObj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = jumpButtonObj.AddComponent<EventTrigger>();
        }
        trigger.triggers.Clear();
        
        // Pointer Down - start jump
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { OnJumpButtonDown(); });
        trigger.triggers.Add(pointerDown);
        
        // Pointer Up - end jump press
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { OnJumpButtonUp(); });
        trigger.triggers.Add(pointerUp);
    }
    
    /// <summary>
    /// Called when jump button is pressed
    /// </summary>
    void OnJumpButtonDown()
    {
        jumpButtonPressed = true;
        
        // Visual feedback - change button color
        if (jumpButtonObj != null)
        {
            Image img = jumpButtonObj.GetComponent<Image>();
            if (img != null) img.color = jumpButtonPressedColor;
        }
    }
    
    /// <summary>
    /// Called when jump button is released
    /// </summary>
    void OnJumpButtonUp()
    {
        jumpButtonPressed = false;
        
        // Reset button color
        if (jumpButtonObj != null)
        {
            Image img = jumpButtonObj.GetComponent<Image>();
            if (img != null) img.color = jumpButtonColor;
        }
    }
    
    /// <summary>
    /// Ensures the jump button exists and is visible after scene transitions.
    /// </summary>
    void EnsureJumpButton()
    {
        // Recreate if destroyed (e.g. scene unload) or never created
        if (jumpButtonObj == null)
        {
            CreateJumpButton();
        }
        else if (!jumpButtonObj.activeInHierarchy)
        {
            jumpButtonObj.SetActive(true);
            EnsureJumpButtonPosition();
        }
        else
        {
            EnsureJumpButtonPosition();
        }
    }

    /// <summary>
    /// Checks if the joystick reference is valid (not null and not destroyed)
    /// </summary>
    bool IsJoystickValid()
    {
        // Check if joystick is null or has been destroyed
        if (joystick == null)
            return false;
        
        // Unity's == operator checks for destroyed objects, but let's be extra safe
        try
        {
            // Try to access a property - this will throw if object is destroyed
            var _ = joystick.gameObject;
            return true;
        }
        catch
        {
            return false;
        }
    }

    void Update()
    {
        // Check if UI is blocking input (either via SetUIOpen or by detecting active popups)
        bool shouldBlockInput = uiOpen || IsUIBlockingInput();
        
        // If popup/UI is open, completely stop touch input processing
        // This prevents joystick from interfering with keyboard/input field
        if (shouldBlockInput)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            lookTouchId = -1;  // Reset look touch
            // DO NOT process ANY touch input while UI is open
            // Movement will still work from keyboard input but not joystick
            return;
        }

        // Process movement (joystick + keyboard)
        HandleMovement();

        // Process camera look (touch on right side of screen)
        HandleTouchLook();
        HandleMouseLook();  // For desktop/editor testing
        HandleJump();
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Get joystick input - this is the PRIMARY input for mobile
        // Re-find joystick if reference is invalid (can happen after scene transition)
        if (!IsJoystickValid())
        {
            FindJoystick();
        }
        
        if (joystick != null)
        {
            float joyX = joystick.Horizontal;
            float joyZ = joystick.Vertical;
            
            // Debug: Log joystick values periodically to help diagnose inversion
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Mathf.Abs(joyZ) > 0.1f && Time.frameCount % 60 == 0)
            {
                Debug.Log($"Joystick: H={joyX:F2}, V={joyZ:F2} | Player forward: {transform.forward}");
            }
            #endif
            
            // Add joystick input to movement
            x += joyX;
            z += joyZ;
        }

        // Clamp to prevent faster diagonal movement
        Vector2 inputVector = new Vector2(x, z);
        if (inputVector.magnitude > 1f)
        {
            inputVector.Normalize();
            x = inputVector.x;
            z = inputVector.y;
        }

        // Apply movement relative to player's facing direction
        // transform.forward is the direction the player is facing
        // transform.right is perpendicular to that
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Handle gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Handle touch-based camera look for iOS/mobile with proper multi-touch support.
    /// Only uses touches on the RIGHT side of the screen (outside joystick zone).
    /// </summary>
    void HandleTouchLook()
    {
        if (Input.touchCount == 0)
        {
            lookTouchId = -1;
            return;
        }

        // Process all touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            
            // Handle new touch - find one on the right side of the screen for look
            if (touch.phase == TouchPhase.Began)
            {
                // Skip if this touch is in the joystick zone (left/bottom)
                if (IsTouchInJoystickZone(touch.position))
                    continue;
                
                // Skip if touching UI elements
                if (IsTouchOverUI(touch.fingerId))
                    continue;
                
                // This touch is valid for camera look
                if (lookTouchId == -1)
                {
                    lookTouchId = touch.fingerId;
                    lastLookTouchPosition = touch.position;
                }
            }
            // Handle movement of the look touch
            else if (touch.fingerId == lookTouchId)
            {
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    // Calculate delta movement
                    Vector2 delta = touch.position - lastLookTouchPosition;
                    lastLookTouchPosition = touch.position;
                    
                    // Apply camera rotation based on touch delta
                    float rotateX = delta.x * touchSensitivity;
                    float rotateY = delta.y * touchSensitivity;
                    
                    // Rotate player (horizontal look)
                    transform.Rotate(Vector3.up * rotateX);
                    
                    // Rotate camera (vertical look)
                    xRotation -= rotateY;
                    xRotation = Mathf.Clamp(xRotation, -80f, 80f);
                    playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    lookTouchId = -1;
                }
            }
        }
    }

    /// <summary>
    /// Handle mouse look for desktop/editor testing only.
    /// On mobile, this is disabled in favor of touch.
    /// </summary>
    void HandleMouseLook()
    {
        // Skip on mobile - use touch instead
        if (Input.touchCount > 0)
            return;
        
        // Skip if clicking on UI
        if (Input.GetMouseButton(0) && EventSystem.current != null && 
            EventSystem.current.IsPointerOverGameObject())
            return;
        
        // Only process if right mouse button is held (for desktop testing)
        // Or if left click is not on UI
        if (Input.GetMouseButton(1) || 
            (Input.GetMouseButton(0) && !IsMouseOverUI()))
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);

            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }
    }

    /// <summary>
    /// Checks if a touch position is in the joystick zone (left/bottom of screen)
    /// </summary>
    private bool IsTouchInJoystickZone(Vector2 touchPosition)
    {
        float screenWidthThreshold = Screen.width * joystickZoneWidth;
        float screenHeightThreshold = Screen.height * joystickZoneHeight;
        
        return touchPosition.x < screenWidthThreshold && 
               touchPosition.y < screenHeightThreshold;
    }

    /// <summary>
    /// Checks if a specific touch is over a UI element using EventSystem
    /// </summary>
    private bool IsTouchOverUI(int fingerId)
    {
        if (EventSystem.current == null)
            return false;
            
        return EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    /// <summary>
    /// Checks if mouse is over UI (for desktop testing)
    /// </summary>
    private bool IsMouseOverUI()
    {
        if (EventSystem.current == null)
            return false;
            
        return EventSystem.current.IsPointerOverGameObject();
    }

    void HandleJump()
    {
        // Check for keyboard jump OR touch button jump
        bool shouldJump = Input.GetButtonDown("Jump") || jumpButtonPressed;
        
        if (shouldJump && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            // Reset button state after jump (prevents continuous jumping while held)
            jumpButtonPressed = false;
        }
    }

    // Called from popup scripts when UI opens/closes
    public void SetUIOpen(bool isOpen)
    {
        uiOpen = isOpen;
        
        if (isOpen)
        {
            lookTouchId = -1;  // Reset look touch when UI opens
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log($"FirstPersonController: SetUIOpen called with {isOpen}");
    }

    /// <summary>
    /// Fallback check to detect if any UI popup is blocking input.
    /// This handles cases where SetUIOpen wasn't called (e.g., after scene transition).
    /// </summary>
    private bool IsUIBlockingInput()
    {
        // Check if there's an active TicketAccessGate popup in the scene
        TicketAccessGate[] gates = FindObjectsOfType<TicketAccessGate>();
        foreach (TicketAccessGate gate in gates)
        {
            // Check if the gate's popup panel is active
            if (gate.popupPanel != null && gate.popupPanel.activeSelf)
            {
                // A popup is active, we should block input
                if (!uiOpen)
                {
                    Debug.LogWarning("FirstPersonController: Detected active popup but uiOpen was false! Auto-blocking input.");
                    uiOpen = true;  // Auto-correct the state
                }
                return true;
            }
        }
        
        // Also check if an input field is currently focused (keyboard is open)
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            // Check if the selected object is an input field
            var inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
            if (inputField != null && inputField.isFocused)
            {
                if (!uiOpen)
                {
                    Debug.LogWarning("FirstPersonController: Detected focused input field but uiOpen was false! Auto-blocking input.");
                    uiOpen = true;
                }
                return true;
            }
        }
        
        return false;
    }
}
