using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TicketAccessGate : MonoBehaviour
{
    [Header("Access Settings")]
    public string correctCode = "GEO7X2";       // رمز التذكرة الصحيح
    public int targetSceneIndex = 1;            // رقم المشهد اللي هينتقل له بعد التحقق
    public GameObject popupPanel;               // البوب أب اللي فيه الإدخال
    public TMP_InputField codeInputField;       // حقل إدخال الرمز
    public TMP_Text feedbackText;               // رسالة الخطأ أو النجاح
    public GameObject gateBlocker;              // جسم يمنع المرور (مثلاً باب أو Collider)
    public Button submitButton;                 // زر الإرسال (Go button)
    
    [Header("Player Controller Reference")]
    [Tooltip("Reference to FirstPersonController to disable movement when popup is open")]
    public FirstPersonController playerController;

    [Header("Popup Size Settings")]
    [Tooltip("Width of the popup panel (0 = use default 600)")]
    public float popupWidth = 600f;
    [Tooltip("Height of the popup panel (0 = use default 400)")]
    public float popupHeight = 400f;

    [Header("UI Styling")]
    public Color inputFieldColor = new Color(1f, 1f, 1f, 1f);  // Pure white background
    public Color inputTextColor = new Color(0.1f, 0.1f, 0.1f, 1f);  // Dark text
    public Color placeholderColor = new Color(0.4f, 0.4f, 0.4f, 1f);  // Visible gray placeholder
    public Color buttonColor = new Color(0.18f, 0.55f, 0.34f, 1f);  // Green button
    public Color buttonTextColor = Color.white;
    public Color feedbackErrorColor = new Color(0.9f, 0.2f, 0.2f, 1f);  // Red for errors
    public Color feedbackSuccessColor = new Color(0.2f, 0.7f, 0.3f, 1f);  // Green for success

    private bool playerNearby = false;
    private bool isProcessingAccess = false;  // Prevent double-triggering
    private float lastAccessAttemptTime = 0f;  // Cooldown for access attempts
    private bool inputFieldWasSelected = false;  // Track if input field was ever selected
    private bool hasInitialized = false;  // Track if Start() has run
    
    // Reference to joystick - we need to disable it when popup is open
    private Joystick joystickToDisable = null;
    private CanvasGroup joystickCanvasGroup = null;
    
    // TouchScreenKeyboard for iOS manual keyboard handling
    private TouchScreenKeyboard mobileKeyboard = null;

    /// <summary>
    /// Called when the script becomes enabled - handles scene transitions
    /// </summary>
    void OnEnable()
    {
        // Reset state flags when re-enabled (scene transitions)
        playerNearby = false;
        isProcessingAccess = false;
        lastAccessAttemptTime = 0f;
        inputFieldWasSelected = false;
        
        // If already initialized (not first run), re-setup critical components
        if (hasInitialized)
        {
            Debug.Log("TicketAccessGate: OnEnable - Re-initializing after scene transition");
            EnsureEventSystemExists();
            FindPlayerController();
            FindSubmitButton();
            
            // Re-find joystick for this scene (old reference may be stale)
            joystickToDisable = null;
            joystickCanvasGroup = null;
            FindJoystick();
        }
    }

    void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
        if (feedbackText != null)
            feedbackText.text = "";
        
        // Ensure EventSystem exists - CRITICAL for iOS keyboard
        EnsureEventSystemExists();
        
        // Find the player controller if not assigned
        FindPlayerController();
        
        // Find the submit button if not assigned
        FindSubmitButton();
        
        // Find joystick so we can disable it when popup is open
        FindJoystick();
        
        // Configure popup panel to be centered and not full-screen
        ConfigurePopupPanel();
        
        // Configure and style UI elements for mobile
        ConfigureUIForMobile();
        
        // Mark as initialized
        hasInitialized = true;
        Debug.Log("TicketAccessGate: Initialized successfully");
    }

    /// <summary>
    /// Ensures an EventSystem exists in the scene - required for keyboard on iOS
    /// </summary>
    void EnsureEventSystemExists()
    {
        // Check if EventSystem.current is valid
        EventSystem currentES = EventSystem.current;
        
        if (currentES == null)
        {
            // Try to find an existing EventSystem in the scene
            EventSystem existingES = FindObjectOfType<EventSystem>();
            
            if (existingES != null)
            {
                // Found one but it's not set as current - this can happen after scene transition
                Debug.Log("TicketAccessGate: Found existing EventSystem, ensuring it's active");
                existingES.gameObject.SetActive(false);
                existingES.gameObject.SetActive(true);
            }
            else
            {
                // No EventSystem exists - create one
                GameObject eventSystemGO = new GameObject("EventSystem_TicketGate");
                EventSystem newES = eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
                Debug.Log("TicketAccessGate: Created new EventSystem for UI input");
            }
        }
        else
        {
            // Ensure the current EventSystem is enabled
            if (!currentES.gameObject.activeInHierarchy)
            {
                currentES.gameObject.SetActive(true);
                Debug.Log("TicketAccessGate: Re-enabled inactive EventSystem");
            }
        }
    }

    /// <summary>
    /// Finds the FirstPersonController to disable movement when popup is open
    /// </summary>
    void FindPlayerController()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<FirstPersonController>();
            if (playerController != null)
            {
                Debug.Log("TicketAccessGate: Found FirstPersonController automatically");
            }
            else
            {
                Debug.LogWarning("TicketAccessGate: No FirstPersonController found - touch input may conflict!");
            }
        }
    }

    /// <summary>
    /// Notifies the player controller that UI is open/closed
    /// Also disables/enables the joystick to prevent it from capturing touches
    /// Returns true if successful, false if player controller not found
    /// </summary>
    bool SetPlayerUIState(bool isOpen)
    {
        // CRITICAL: Disable/enable joystick to prevent it from capturing touches
        SetJoystickInteractable(!isOpen);
        
        if (playerController != null)
        {
            playerController.SetUIOpen(isOpen);
            Debug.Log($"TicketAccessGate: Set player UI state to {isOpen}, joystick interactable: {!isOpen}");
            return true;
        }
        else
        {
            Debug.LogError($"TicketAccessGate: playerController is NULL! Cannot set UI state to {isOpen}");
            return false;
        }
    }
    
    /// <summary>
    /// Finds the joystick in the scene and stores reference for later control
    /// </summary>
    void FindJoystick()
    {
        if (joystickToDisable == null)
        {
            joystickToDisable = FindObjectOfType<Joystick>();
            if (joystickToDisable != null)
            {
                // Get or add a CanvasGroup to control interactability
                joystickCanvasGroup = joystickToDisable.GetComponent<CanvasGroup>();
                if (joystickCanvasGroup == null)
                {
                    joystickCanvasGroup = joystickToDisable.gameObject.AddComponent<CanvasGroup>();
                }
                Debug.Log("TicketAccessGate: Found joystick for control");
            }
        }
    }
    
    /// <summary>
    /// Enables or disables the joystick's ability to receive touches
    /// This is CRITICAL - the joystick's IPointerDownHandler intercepts touches meant for the input field
    /// </summary>
    void SetJoystickInteractable(bool interactable)
    {
        // Always try to find joystick if we don't have a reference
        if (joystickToDisable == null)
        {
            FindJoystick();
        }
        
        if (joystickCanvasGroup != null)
        {
            // CanvasGroup.blocksRaycasts controls whether this UI element receives touch events
            joystickCanvasGroup.blocksRaycasts = interactable;
            joystickCanvasGroup.interactable = interactable;
            Debug.Log($"TicketAccessGate: Joystick interactable set to {interactable}");
        }
        else if (joystickToDisable != null)
        {
            // Fallback: disable the entire joystick GameObject
            joystickToDisable.gameObject.SetActive(interactable);
            Debug.Log($"TicketAccessGate: Joystick GameObject active set to {interactable}");
        }
    }

    /// <summary>
    /// Finds the submit button in the popup if not manually assigned and connects the click handler
    /// </summary>
    void FindSubmitButton()
    {
        if (submitButton == null && popupPanel != null)
        {
            // Try to find a button in the popup panel
            submitButton = popupPanel.GetComponentInChildren<Button>();
        }
        
        // CRITICAL: Connect the button click to TryAccess method
        if (submitButton != null)
        {
            // Ensure button is interactable
            submitButton.interactable = true;
            
            // Ensure raycast is enabled on the button image
            Image buttonImage = submitButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true;
            }
            
            // Remove any existing listeners to avoid duplicates
            submitButton.onClick.RemoveAllListeners();
            
            // Add the click handler
            submitButton.onClick.AddListener(OnSubmitButtonClicked);
            
            Debug.Log("TicketAccessGate: Submit button connected to OnSubmitButtonClicked()");
        }
        else
        {
            Debug.LogError("TicketAccessGate: No submit button found! Button clicks will not work.");
        }
    }

    /// <summary>
    /// Called when the submit button is clicked. This wrapper ensures the click is processed.
    /// </summary>
    void OnSubmitButtonClicked()
    {
        Debug.Log("TicketAccessGate: Submit button clicked!");
        TryAccess();
    }

    /// <summary>
    /// Configures the popup panel to be centered with a fixed size instead of full-screen
    /// </summary>
    void ConfigurePopupPanel()
    {
        if (popupPanel == null) return;
        
        RectTransform popupRect = popupPanel.GetComponent<RectTransform>();
        if (popupRect != null)
        {
            // Set anchors to center
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Set fixed size (not full-screen)
            float width = popupWidth > 0 ? popupWidth : 600f;
            float height = popupHeight > 0 ? popupHeight : 400f;
            popupRect.sizeDelta = new Vector2(width, height);
            
            // Center the popup
            popupRect.anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// Configures and styles all UI elements for mobile
    /// </summary>
    void ConfigureUIForMobile()
    {
        if (popupPanel == null) return;
        
        float panelWidth = popupWidth > 0 ? popupWidth : 600f;
        
        // Configure Input Field
        ConfigureInputField(panelWidth);
        
        // Configure Submit Button
        ConfigureSubmitButton(panelWidth);
        
        // Configure Feedback Text
        ConfigureFeedbackText(panelWidth);
    }

    /// <summary>
    /// Configures and styles the input field
    /// </summary>
    void ConfigureInputField(float panelWidth)
    {
        if (codeInputField == null) return;
        
        RectTransform inputRect = codeInputField.GetComponent<RectTransform>();
        if (inputRect != null)
        {
            // Set anchors to bottom of popup (below the card image)
            inputRect.anchorMin = new Vector2(0.5f, 0f);
            inputRect.anchorMax = new Vector2(0.5f, 0f);
            inputRect.pivot = new Vector2(0.5f, 0f);
            
            // Size: 85% of panel width, good touch target height
            float inputWidth = panelWidth * 0.85f;
            float inputHeight = 50f;
            inputRect.sizeDelta = new Vector2(inputWidth, inputHeight);
            
            // Position from bottom of popup
            inputRect.anchoredPosition = new Vector2(0, 100f);
        }
        
        // Style the input field background - ensure high visibility
        Image inputImage = codeInputField.GetComponent<Image>();
        if (inputImage != null)
        {
            inputImage.color = inputFieldColor;  // Pure white
            inputImage.raycastTarget = true;
            inputImage.type = Image.Type.Sliced;
        }
        
        // Style the actual input text - make it dark and visible
        TMP_Text textComponent = codeInputField.textComponent;
        if (textComponent != null)
        {
            textComponent.fontSize = 22;
            textComponent.color = inputTextColor;  // Dark color for visibility
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontStyle = FontStyles.Normal;
        }
        
        // Style the placeholder - make it clearly visible but distinct
        if (codeInputField.placeholder != null)
        {
            TMP_Text placeholder = codeInputField.placeholder as TMP_Text;
            if (placeholder != null)
            {
                placeholder.fontSize = 20;
                placeholder.color = placeholderColor;  // Darker gray, fully opaque
                placeholder.alignment = TextAlignmentOptions.Center;
                placeholder.fontStyle = FontStyles.Italic;
                placeholder.text = "Enter Code...";  // Ensure placeholder text
            }
        }
        
        // Configure input field settings
        codeInputField.characterLimit = 20;
        codeInputField.contentType = TMP_InputField.ContentType.Alphanumeric;
        codeInputField.keyboardType = TouchScreenKeyboardType.Default;
        
        // iOS-specific settings
        #if UNITY_IOS || UNITY_ANDROID
        // shouldHideMobileInput = false shows the native input field which works better on iOS
        codeInputField.shouldHideMobileInput = false;
        Debug.Log("TicketAccessGate: Configured input field for mobile (shouldHideMobileInput=false)");
        #endif
        
        // Make the entire input field touchable
        MakeInputFieldFullyTouchable();
    }

    /// <summary>
    /// Makes the entire input field area responsive to touch
    /// </summary>
    void MakeInputFieldFullyTouchable()
    {
        if (codeInputField == null) return;
        
        // Ensure raycast target on main image
        Image inputImage = codeInputField.GetComponent<Image>();
        if (inputImage != null)
        {
            inputImage.raycastTarget = true;
        }
        
        // Configure Text Area child - DO NOT add components at runtime (causes iOS crashes)
        Transform textArea = codeInputField.transform.Find("Text Area");
        if (textArea != null)
        {
            // Ensure RectTransform fills the entire input field
            RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
            if (textAreaRect != null)
            {
                textAreaRect.anchorMin = Vector2.zero;
                textAreaRect.anchorMax = Vector2.one;
                textAreaRect.offsetMin = new Vector2(10, 5);
                textAreaRect.offsetMax = new Vector2(-10, -5);
            }
            
            // Only configure existing Image component, don't add new ones
            Image textAreaImage = textArea.GetComponent<Image>();
            if (textAreaImage != null)
            {
                textAreaImage.color = new Color(0, 0, 0, 0);
                textAreaImage.raycastTarget = true;
            }
        }
    }

    /// <summary>
    /// Configures and styles the submit button
    /// </summary>
    void ConfigureSubmitButton(float panelWidth)
    {
        if (submitButton == null) return;
        
        RectTransform buttonRect = submitButton.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            // Set anchors to bottom of popup
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            
            // Size: mobile-friendly button size
            float buttonWidth = panelWidth * 0.6f;
            float buttonHeight = 45f;
            buttonRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            
            // Position above the feedback text, below input field
            buttonRect.anchoredPosition = new Vector2(0, 45f);
        }
        
        // Style the button background
        Image buttonImage = submitButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = buttonColor;
            buttonImage.raycastTarget = true;
            buttonImage.type = Image.Type.Sliced;
        }
        
        // Style the button text - keep existing text (may be Arabic)
        TMP_Text buttonText = submitButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            // Don't change text - keep localized text if present
            buttonText.fontSize = 20;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.color = buttonTextColor;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.enableWordWrapping = false;
            buttonText.overflowMode = TextOverflowModes.Overflow;
        }
        else
        {
            // Try legacy Text component
            Text legacyText = submitButton.GetComponentInChildren<Text>();
            if (legacyText != null)
            {
                legacyText.fontSize = 20;
                legacyText.fontStyle = FontStyle.Bold;
                legacyText.color = buttonTextColor;
                legacyText.alignment = TextAnchor.MiddleCenter;
            }
        }
        
        // Configure button colors for interaction feedback
        ColorBlock colors = submitButton.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = new Color(
            Mathf.Min(buttonColor.r * 1.2f, 1f), 
            Mathf.Min(buttonColor.g * 1.2f, 1f), 
            Mathf.Min(buttonColor.b * 1.2f, 1f), 1f);
        colors.pressedColor = new Color(
            buttonColor.r * 0.7f, 
            buttonColor.g * 0.7f, 
            buttonColor.b * 0.7f, 1f);
        colors.selectedColor = buttonColor;
        colors.fadeDuration = 0.1f;
        submitButton.colors = colors;
    }

    /// <summary>
    /// Configures the feedback text position and style
    /// </summary>
    void ConfigureFeedbackText(float panelWidth)
    {
        if (feedbackText == null) return;
        
        RectTransform feedbackRect = feedbackText.GetComponent<RectTransform>();
        if (feedbackRect != null)
        {
            // Set anchors to bottom of popup
            feedbackRect.anchorMin = new Vector2(0.5f, 0f);
            feedbackRect.anchorMax = new Vector2(0.5f, 0f);
            feedbackRect.pivot = new Vector2(0.5f, 0f);
            
            // Size and position - at the very bottom
            feedbackRect.sizeDelta = new Vector2(panelWidth * 0.9f, 35f);
            feedbackRect.anchoredPosition = new Vector2(0, 5f);
        }
        
        // Style the text for visibility
        feedbackText.fontSize = 16;
        feedbackText.fontStyle = FontStyles.Bold;
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.color = feedbackErrorColor;  // Default to error color
        feedbackText.enableWordWrapping = true;
        feedbackText.overflowMode = TextOverflowModes.Ellipsis;
        
        // Add outline/shadow for better visibility if possible
        // This is done by ensuring the text stands out
    }

    /// <summary>
    /// Shows feedback message with appropriate styling
    /// </summary>
    void ShowFeedback(string message, bool isSuccess)
    {
        if (feedbackText == null) return;
        
        feedbackText.text = message;
        feedbackText.color = isSuccess ? feedbackSuccessColor : feedbackErrorColor;
        
        // Make sure the text object is active
        feedbackText.gameObject.SetActive(true);
    }

    void Update()
    {
        // Safety check
        if (popupPanel == null) return;
        
        // Sync text from manually opened TouchScreenKeyboard (iOS)
        #if UNITY_IOS || UNITY_ANDROID
        if (mobileKeyboard != null)
        {
            if (mobileKeyboard.status == TouchScreenKeyboard.Status.Visible)
            {
                // Sync text while typing
                if (codeInputField != null && codeInputField.text != mobileKeyboard.text)
                {
                    codeInputField.text = mobileKeyboard.text;
                }
            }
            else if (mobileKeyboard.status == TouchScreenKeyboard.Status.Done)
            {
                // Keyboard closed with "Done" - submit
                if (codeInputField != null)
                {
                    codeInputField.text = mobileKeyboard.text;
                }
                mobileKeyboard = null;
                TryAccess();
            }
            else if (mobileKeyboard.status == TouchScreenKeyboard.Status.Canceled || 
                     mobileKeyboard.status == TouchScreenKeyboard.Status.LostFocus)
            {
                // Keyboard was dismissed without submitting - reset input field state
                Debug.Log("TicketAccessGate: Keyboard dismissed externally, resetting input field state");
                if (codeInputField != null)
                {
                    codeInputField.text = mobileKeyboard.text;  // Keep typed text
                    codeInputField.DeactivateInputField();
                }
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
                mobileKeyboard = null;
                inputFieldWasSelected = false;
            }
        }
        
        // CRITICAL: Detect when iOS keyboard was closed externally (user pressed keyboard dismiss button)
        // The input field might think it's still focused even though keyboard is gone
        if (codeInputField != null && codeInputField.isFocused && !TouchScreenKeyboard.visible)
        {
            Debug.Log("TicketAccessGate: Input field thinks it's focused but keyboard not visible - resetting");
            codeInputField.DeactivateInputField();
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            inputFieldWasSelected = false;
        }
        #endif
        
        // If player nearby and pressed Enter
        if (playerNearby && Input.GetKeyDown(KeyCode.Return))
        {
            TryAccess();
        }
        
        // Handle touch input for iOS - activate input field and button when touched
        if (playerNearby && popupPanel.activeSelf)
        {
            try
            {
                HandleTouchInput();
                HandleButtonTouchFallback();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"TicketAccessGate: Touch handling error: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Fallback touch handler for the submit button on iOS.
    /// Sometimes Unity Button.onClick doesn't fire reliably on iOS, so we detect touches manually.
    /// </summary>
    void HandleButtonTouchFallback()
    {
        if (submitButton == null) return;
        
        // Check for touch input (iOS)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            // Only trigger on touch up (finger lifted) to prevent double-firing
            if (touch.phase == TouchPhase.Ended)
            {
                if (IsTouchOverButton(touch.position))
                {
                    Debug.Log("TicketAccessGate: Button touched via fallback handler");
                    TryAccess();
                }
            }
        }
    }

    /// <summary>
    /// Checks if a screen position is over the submit button
    /// </summary>
    bool IsTouchOverButton(Vector2 screenPosition)
    {
        if (submitButton == null) return false;
        
        RectTransform buttonRect = submitButton.GetComponent<RectTransform>();
        if (buttonRect == null) return false;
        
        // Get the canvas for proper coordinate conversion
        Canvas canvas = submitButton.GetComponentInParent<Canvas>();
        if (canvas == null) return false;
        
        Camera cam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
        {
            cam = canvas.worldCamera;
        }
        
        return RectTransformUtility.RectangleContainsScreenPoint(buttonRect, screenPosition, cam);
    }

    /// <summary>
    /// Handles touch input to ensure the input field activates properly on iOS
    /// </summary>
    void HandleTouchInput()
    {
        if (codeInputField == null) return;
        
        // Check for touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                // Check if touch is within the input field area
                if (IsTouchOverInputField(touch.position))
                {
                    Debug.Log($"TicketAccessGate: Touch detected on input field. isFocused={codeInputField.isFocused}, keyboardVisible={TouchScreenKeyboard.visible}");
                    
                    // CRITICAL: If keyboard is not visible but input thinks it's focused, reset first
                    #if UNITY_IOS || UNITY_ANDROID
                    if (codeInputField.isFocused && !TouchScreenKeyboard.visible)
                    {
                        Debug.Log("TicketAccessGate: Resetting stuck focus state before activation");
                        codeInputField.DeactivateInputField();
                        if (EventSystem.current != null)
                        {
                            EventSystem.current.SetSelectedGameObject(null);
                        }
                    }
                    #endif
                    
                    ActivateInputFieldSafe();
                }
            }
        }
        // Also handle mouse clicks for editor testing
        else if (Input.GetMouseButtonDown(0))
        {
            if (IsTouchOverInputField(Input.mousePosition))
            {
                Debug.Log("TicketAccessGate: Mouse click detected on input field");
                ActivateInputFieldSafe();
            }
        }
    }

    /// <summary>
    /// Safely activates the input field with proper EventSystem handling for iOS
    /// </summary>
    void ActivateInputFieldSafe()
    {
        if (codeInputField == null) return;
        
        Debug.Log("TicketAccessGate: ActivateInputFieldSafe called");
        
        // CRITICAL: Ensure EventSystem exists and is working
        EnsureEventSystemExists();
        
        // Stop any pending activation coroutines
        StopAllCoroutines();
        
        // Start robust activation sequence
        StartCoroutine(ActivateInputFieldCoroutine());
    }

    /// <summary>
    /// Robust coroutine to activate input field on iOS - handles scene transition issues
    /// Uses multiple methods to ensure keyboard opens
    /// </summary>
    System.Collections.IEnumerator ActivateInputFieldCoroutine()
    {
        Debug.Log("TicketAccessGate: Starting input field activation coroutine");
        
        // Wait one frame for any pending UI updates
        yield return null;
        
        if (codeInputField == null || popupPanel == null || !popupPanel.activeSelf)
        {
            Debug.Log("TicketAccessGate: Activation cancelled - popup not active");
            yield break;
        }
        
        // STEP 1: Ensure EventSystem is ready
        EnsureEventSystemExists();
        
        if (EventSystem.current == null)
        {
            Debug.LogError("TicketAccessGate: No EventSystem available for keyboard!");
            // Try to create one
            GameObject esGO = new GameObject("EventSystem_Emergency");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
            yield return null;
        }
        
        // STEP 2: Force complete reset of input field state
        Debug.Log("TicketAccessGate: Forcing complete input field reset");
        
        // Deactivate first
        codeInputField.DeactivateInputField();
        codeInputField.ReleaseSelection();
        
        // Clear EventSystem selection
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        yield return null;
        
        // STEP 3: Reset component state by toggling enabled
        codeInputField.interactable = false;
        codeInputField.enabled = false;
        yield return null;
        codeInputField.enabled = true;
        codeInputField.interactable = true;
        yield return null;
        yield return null;  // Extra frame for iOS
        
        // STEP 4: Select and activate the input field using multiple methods
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(codeInputField.gameObject);
        }
        codeInputField.Select();
        codeInputField.ActivateInputField();
        
        Debug.Log($"TicketAccessGate: Input field activated. isFocused={codeInputField.isFocused}");
        
        // STEP 5: For iOS - use TouchScreenKeyboard directly if needed
        #if UNITY_IOS || UNITY_ANDROID
        yield return new WaitForSeconds(0.15f);
        
        if (codeInputField != null && popupPanel != null && popupPanel.activeSelf)
        {
            if (!codeInputField.isFocused || !TouchScreenKeyboard.visible)
            {
                Debug.Log("TicketAccessGate: Keyboard not visible, opening manually");
                
                // Force focus again
                codeInputField.Select();
                codeInputField.ActivateInputField();
                
                // If still no keyboard, try opening it directly
                yield return new WaitForSeconds(0.1f);
                if (!TouchScreenKeyboard.visible && codeInputField != null)
                {
                    Debug.Log("TicketAccessGate: Opening TouchScreenKeyboard directly");
                    mobileKeyboard = TouchScreenKeyboard.Open(
                        codeInputField.text, 
                        TouchScreenKeyboardType.Default, 
                        false,  // autocorrection
                        false,  // multiline
                        false,  // secure
                        false,  // alert
                        "Enter access code",  // placeholder
                        50      // max length
                    );
                }
            }
        }
        #else
        // For editor/desktop, just retry activation
        yield return new WaitForSeconds(0.1f);
        if (codeInputField != null && popupPanel != null && popupPanel.activeSelf && !codeInputField.isFocused)
        {
            Debug.Log("TicketAccessGate: Re-activating input field (was not focused)");
            codeInputField.Select();
            codeInputField.ActivateInputField();
        }
        #endif
        
        inputFieldWasSelected = true;
        Debug.Log($"TicketAccessGate: Activation complete. isFocused={codeInputField?.isFocused}, keyboard visible={TouchScreenKeyboard.visible}");
    }

    /// <summary>
    /// Checks if a screen position is over the input field
    /// </summary>
    bool IsTouchOverInputField(Vector2 screenPosition)
    {
        if (codeInputField == null) return false;
        
        RectTransform inputRect = codeInputField.GetComponent<RectTransform>();
        if (inputRect == null) return false;
        
        // Get the canvas for proper coordinate conversion
        Canvas canvas = codeInputField.GetComponentInParent<Canvas>();
        if (canvas == null) return false;
        
        Camera cam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
        {
            cam = canvas.worldCamera;
        }
        
        return RectTransformUtility.RectangleContainsScreenPoint(inputRect, screenPosition, cam);
    }

    public void TryAccess()
    {
        // Prevent double-triggering with a short cooldown
        if (Time.time - lastAccessAttemptTime < 0.3f)
        {
            Debug.Log("TicketAccessGate: Ignoring duplicate access attempt");
            return;
        }
        lastAccessAttemptTime = Time.time;
        
        if (isProcessingAccess) return;
        if (codeInputField == null) return;
        
        string enteredCode = codeInputField.text.Trim();
        
        Debug.Log($"TicketAccessGate: TryAccess called with code: '{enteredCode}'");

        // Check if code is empty
        if (string.IsNullOrEmpty(enteredCode))
        {
            ShowFeedback("Please enter a code", false);
            return;
        }

        if (enteredCode.Equals(correctCode, System.StringComparison.OrdinalIgnoreCase))
        {
            isProcessingAccess = true;
            ShowFeedback("✓ Verified! Loading...", true);
            
            if (gateBlocker != null)
                gateBlocker.SetActive(false);

            Debug.Log($"TicketAccessGate: Code correct! Loading scene {targetSceneIndex}");
            
            // Load scene asynchronously (prevents freeze on mobile)
            SceneManager.LoadSceneAsync(targetSceneIndex);
        }
        else
        {
            ShowFeedback("✗ Invalid code. Try again.", false);
            Debug.Log($"TicketAccessGate: Code incorrect. Expected: {correctCode}");
            
            // Clear the input field for retry
            codeInputField.text = "";
            codeInputField.Select();
            codeInputField.ActivateInputField();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            
            // Safety check for popup panel
            if (popupPanel == null)
            {
                Debug.LogError("TicketAccessGate: popupPanel is null!");
                return;
            }
            
            // CRITICAL: Re-find player controller EVERY time popup opens
            // This handles scene transitions where the old reference becomes invalid
            FindPlayerControllerNow();
            
            // Ensure EventSystem exists BEFORE showing popup
            EnsureEventSystemExists();
            
            popupPanel.SetActive(true);
            
            // CRITICAL: Tell player controller that UI is open to stop touch processing
            if (!SetPlayerUIState(true))
            {
                Debug.LogWarning("TicketAccessGate: Could not disable player input - player may move while typing!");
            }
            
            // Reset all state flags
            isProcessingAccess = false;
            lastAccessAttemptTime = 0f;
            inputFieldWasSelected = false;
            
            // Stop all existing coroutines to prevent conflicts
            StopAllCoroutines();
            
            // Reset the UI state
            if (feedbackText != null)
            {
                feedbackText.text = "";
                feedbackText.gameObject.SetActive(true);
            }
            
            if (codeInputField != null)
            {
                codeInputField.text = "";
                codeInputField.interactable = true;
                
                // Ensure the input field is ready to receive input
                if (EventSystem.current != null)
                {
                    // Clear any existing selection first
                    EventSystem.current.SetSelectedGameObject(null);
                }
                
                // DO NOT auto-focus - let user tap to open keyboard
                // This prevents unwanted keyboard popup and iOS issues
                // The user will tap the input field to open keyboard
            }
            
            Debug.Log("TicketAccessGate: Player entered, popup activated - tap input field to type");
        }
    }

    /// <summary>
    /// Finds the player controller immediately - called every time popup opens
    /// to handle scene transitions where the old reference becomes invalid
    /// </summary>
    void FindPlayerControllerNow()
    {
        // Always search for the player controller, even if we have a reference
        // because the old reference might be from a destroyed object
        FirstPersonController foundController = FindObjectOfType<FirstPersonController>();
        
        if (foundController != null)
        {
            playerController = foundController;
            Debug.Log("TicketAccessGate: Found FirstPersonController for this scene");
        }
        else
        {
            playerController = null;
            Debug.LogError("TicketAccessGate: NO FirstPersonController found in scene! Player will move while popup is open!");
        }
    }

    /// <summary>
    /// Delays input field focus slightly to ensure it works on iOS
    /// </summary>
    System.Collections.IEnumerator DelayedInputFieldFocus()
    {
        // Wait multiple frames for iOS to fully set up the UI
        yield return null;
        yield return null;
        yield return new WaitForSeconds(0.1f);
        
        if (codeInputField != null && popupPanel != null && popupPanel.activeSelf)
        {
            // Ensure EventSystem knows about our input field
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(codeInputField.gameObject);
            }
            
            codeInputField.Select();
            codeInputField.ActivateInputField();
            inputFieldWasSelected = true;
            
            Debug.Log("TicketAccessGate: Input field focused after delay (iOS-safe)");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            
            // Stop any pending coroutines
            StopAllCoroutines();
            
            // Deactivate input field to hide keyboard
            if (codeInputField != null)
            {
                codeInputField.DeactivateInputField();
                codeInputField.text = "";  // Clear text for next time
            }
            
            // Clean up mobile keyboard reference
            #if UNITY_IOS || UNITY_ANDROID
            if (mobileKeyboard != null)
            {
                mobileKeyboard = null;
            }
            #endif
            
            // Clear EventSystem selection
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            
            if (popupPanel != null)
                popupPanel.SetActive(false);
            
            // Re-enable player movement
            SetPlayerUIState(false);
            
            // Reset state
            inputFieldWasSelected = false;
            
            Debug.Log("TicketAccessGate: Player exited, popup closed");
        }
    }

    /// <summary>
    /// Called when the script is disabled or destroyed
    /// </summary>
    void OnDisable()
    {
        StopAllCoroutines();
        
        // Clean up UI state
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        // Re-enable player movement and joystick
        SetPlayerUIState(false);
        
        // Extra safety: always ensure joystick is re-enabled when this script is disabled
        SetJoystickInteractable(true);
    }
}
