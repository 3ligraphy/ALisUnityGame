using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    void Start()
    {
        popupPanel.SetActive(false);
        feedbackText.text = "";
        
        // Find the submit button if not assigned
        FindSubmitButton();
        
        // Configure popup panel to be centered and not full-screen
        ConfigurePopupPanel();
        
        // Configure and style UI elements for mobile
        ConfigureUIForMobile();
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
        
        // Configure Text Area child
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
            
            // Add invisible image for touch detection
            Image textAreaImage = textArea.GetComponent<Image>();
            if (textAreaImage == null)
            {
                textAreaImage = textArea.gameObject.AddComponent<Image>();
            }
            textAreaImage.color = new Color(0, 0, 0, 0);
            textAreaImage.raycastTarget = true;
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
        // If player nearby and pressed Enter
        if (playerNearby && Input.GetKeyDown(KeyCode.Return))
        {
            TryAccess();
        }
        
        // Handle touch input for iOS - activate input field and button when touched
        if (playerNearby && popupPanel.activeSelf)
        {
            HandleTouchInput();
            HandleButtonTouchFallback();
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
                    // Activate the input field
                    codeInputField.Select();
                    codeInputField.ActivateInputField();
                }
            }
        }
        // Also handle mouse clicks for editor testing
        else if (Input.GetMouseButtonDown(0))
        {
            if (IsTouchOverInputField(Input.mousePosition))
            {
                codeInputField.Select();
                codeInputField.ActivateInputField();
            }
        }
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
            popupPanel.SetActive(true);
            
            // Reset all state flags
            isProcessingAccess = false;
            lastAccessAttemptTime = 0f;
            
            // Reset the UI state
            if (feedbackText != null)
            {
                feedbackText.text = "";
                feedbackText.gameObject.SetActive(true);
            }
            
            if (codeInputField != null)
            {
                codeInputField.text = "";
                // Auto-focus the input field for better UX
                codeInputField.Select();
                codeInputField.ActivateInputField();
            }
            
            Debug.Log("TicketAccessGate: Player entered, popup activated");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            popupPanel.SetActive(false);
        }
    }
}
