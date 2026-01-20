using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Ticket Access Gate - Original popup preserved, floating numeric keypad with display
/// </summary>
public class TicketAccessGate : MonoBehaviour
{
    [Header("Access Settings")]
    public string correctCode = "GEO7X2";
    public int targetSceneIndex = 1;
    public GameObject popupPanel;
    public TMP_InputField codeInputField;
    public TMP_Text feedbackText;
    public GameObject gateBlocker;
    public Button submitButton;
    
    [Header("Player Controller Reference")]
    public FirstPersonController playerController;

    [Header("Keypad Settings")]
    public float buttonSize = 60f;
    public float buttonSpacing = 5f;

    [Header("Keypad Colors")]
    public Color keypadBgColor = new Color(0.12f, 0.12f, 0.15f, 0.95f);
    public Color displayBgColor = new Color(0.05f, 0.05f, 0.08f, 1f);
    public Color displayTextColor = new Color(0.4f, 0.9f, 0.5f, 1f);
    public Color numberButtonColor = new Color(0.25f, 0.25f, 0.3f, 1f);
    public Color fillButtonColor = new Color(0.2f, 0.5f, 0.35f, 1f);
    public Color clearButtonColor = new Color(0.5f, 0.4f, 0.2f, 1f);
    public Color deleteButtonColor = new Color(0.5f, 0.25f, 0.25f, 1f);

    // Private state
    private bool playerNearby = false;
    private bool isProcessingAccess = false;
    private bool hasInitialized = false;
    private string keypadText = "";
    private const int MAX_CODE_LENGTH = 10;
    
    // Keypad UI
    private GameObject keypadPanel;
    private TMP_Text keypadDisplayText;
    private bool keypadVisible = false;
    private Button openKeypadButton;
    
    // Joystick reference
    private Joystick joystickToDisable = null;
    private CanvasGroup joystickCanvasGroup = null;

    void OnEnable()
    {
        playerNearby = false;
        isProcessingAccess = false;
        keypadText = "";
        keypadVisible = false;
        
        if (hasInitialized)
        {
            Debug.Log("TicketAccessGate: OnEnable - reinitializing");
            
            // Re-find references (they may be stale after scene load)
            FindPlayerController();
            FindJoystick();
            FindSubmitButton();
            
            // Ensure EventSystem is active
            EnsureEventSystem();
            
            // Fix popup size again
            FixPopupSize();
            
            // Re-setup submit button
            SetupOriginalSubmitButton();
            
            if (keypadPanel != null) keypadPanel.SetActive(false);
        }
    }

    void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
        if (feedbackText != null)
            feedbackText.text = "";
        
        FindPlayerController();
        FindJoystick();
        FindSubmitButton();
        
        // FORCE fix popup size - don't rely on detection
        FixPopupSize();
        
        // Ensure EventSystem exists (critical for buttons to work after scene load)
        EnsureEventSystem();
        
        // Setup the original submit button (DON'T change its size/position)
        SetupOriginalSubmitButton();
        
        // Create the open keypad button
        CreateOpenKeypadButton();
        
        // Create keypad with display
        CreateKeypadWithDisplay();
        
        hasInitialized = true;
        Debug.Log("TicketAccessGate: Initialized");
    }
    
    void FixPopupSize()
    {
        if (popupPanel == null) return;
        
        RectTransform rect = popupPanel.GetComponent<RectTransform>();
        if (rect == null) return;
        
        // Always set to centered, reasonable size
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(500, 380);
        
        // Also fix input field and button sizes to fit within popup
        FixInputFieldSize();
        FixSubmitButtonSize();
        
        Debug.Log("TicketAccessGate: Fixed popup, input field, and button sizes");
    }
    
    void FixInputFieldSize()
    {
        if (codeInputField == null) return;
        
        RectTransform inputRect = codeInputField.GetComponent<RectTransform>();
        if (inputRect == null) return;
        
        // Position input field in lower portion of popup, reasonable size
        inputRect.anchorMin = new Vector2(0.5f, 0);
        inputRect.anchorMax = new Vector2(0.5f, 0);
        inputRect.pivot = new Vector2(0.5f, 0);
        inputRect.anchoredPosition = new Vector2(0, 80);  // 80px from bottom
        inputRect.sizeDelta = new Vector2(280, 45);  // Reasonable size for input
        
        // Style the input field
        if (codeInputField.textComponent != null)
        {
            codeInputField.textComponent.fontSize = 18;
        }
        
        // Fix placeholder
        if (codeInputField.placeholder != null)
        {
            TMP_Text placeholder = codeInputField.placeholder as TMP_Text;
            if (placeholder != null)
            {
                placeholder.fontSize = 16;
            }
        }
    }
    
    void FixSubmitButtonSize()
    {
        if (submitButton == null) return;
        
        RectTransform buttonRect = submitButton.GetComponent<RectTransform>();
        if (buttonRect == null) return;
        
        // Position button below input field
        buttonRect.anchorMin = new Vector2(0.5f, 0);
        buttonRect.anchorMax = new Vector2(0.5f, 0);
        buttonRect.pivot = new Vector2(0.5f, 0);
        buttonRect.anchoredPosition = new Vector2(0, 25);  // 25px from bottom
        buttonRect.sizeDelta = new Vector2(120, 45);  // Reasonable button size
        
        // Style button text
        TMP_Text buttonText = submitButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.fontSize = 18;
        }
    }
    
    void EnsureEventSystem()
    {
        // Check if EventSystem exists
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.Log("TicketAccessGate: Creating EventSystem");
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }
        else if (!eventSystem.gameObject.activeInHierarchy)
        {
            Debug.Log("TicketAccessGate: Activating EventSystem");
            eventSystem.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (popupPanel == null || !popupPanel.activeSelf) return;
        
        if (playerNearby && Input.GetKeyDown(KeyCode.Return))
        {
            if (keypadVisible)
                FillInputAndClose();
            else
                TryAccess();
        }
        
        // Desktop keyboard input
        if (playerNearby && keypadVisible)
        {
            foreach (char c in Input.inputString)
            {
                if (char.IsDigit(c))
                    AddDigit(c);
                else if (c == '\b')
                    RemoveLastDigit();
            }
        }
    }

    void SetupOriginalSubmitButton()
    {
        if (submitButton == null) return;
        
        // Just wire up the click - DON'T change size or position
        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(TryAccess);
    }

    void CreateOpenKeypadButton()
    {
        if (codeInputField == null) return;
        
        // Check if button already exists
        if (openKeypadButton != null) return;
        
        // Also check by name
        Transform existing = codeInputField.transform.Find("OpenKeypadBtn");
        if (existing != null)
        {
            openKeypadButton = existing.GetComponent<Button>();
            if (openKeypadButton != null)
            {
                openKeypadButton.onClick.RemoveAllListeners();
                openKeypadButton.onClick.AddListener(ToggleKeypad);
                return;
            }
        }
        
        // Small button as child of input field
        GameObject buttonObj = new GameObject("OpenKeypadBtn");
        buttonObj.transform.SetParent(codeInputField.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 0);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.pivot = new Vector2(1, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0, 0);
        buttonRect.sizeDelta = new Vector2(45, 0);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.5f, 0.65f, 1f);
        
        openKeypadButton = buttonObj.AddComponent<Button>();
        openKeypadButton.onClick.AddListener(ToggleKeypad);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TMP_Text btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "123";
        btnText.fontSize = 14;
        btnText.fontStyle = FontStyles.Bold;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
    }

    void CreateKeypadWithDisplay()
    {
        if (popupPanel == null) return;
        
        // Check if keypad already exists
        if (keypadPanel != null) return;
        
        Canvas parentCanvas = popupPanel.GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;
        
        // Also check by name
        Transform existing = parentCanvas.transform.Find("NumericKeypad");
        if (existing != null)
        {
            keypadPanel = existing.gameObject;
            keypadDisplayText = keypadPanel.GetComponentInChildren<TMP_Text>();
            return;
        }
        
        // Calculate sizes
        float gridWidth = 3 * buttonSize + 2 * buttonSpacing;
        float gridHeight = 4 * buttonSize + 3 * buttonSpacing;
        float displayHeight = 50f;
        float fillButtonHeight = 45f;
        float padding = 15f;
        float totalHeight = displayHeight + gridHeight + fillButtonHeight + padding * 4;
        float totalWidth = gridWidth + padding * 2;
        
        // Create panel with background
        keypadPanel = new GameObject("NumericKeypad");
        keypadPanel.transform.SetParent(parentCanvas.transform, false);
        
        RectTransform panelRect = keypadPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0.5f);
        panelRect.anchorMax = new Vector2(1, 0.5f);
        panelRect.pivot = new Vector2(1, 0.5f);
        panelRect.sizeDelta = new Vector2(totalWidth, totalHeight);
        panelRect.anchoredPosition = new Vector2(-20, 0);
        
        Image panelBg = keypadPanel.AddComponent<Image>();
        panelBg.color = keypadBgColor;
        
        // Create display area at top
        CreateKeypadDisplay(padding, totalHeight - padding);
        
        // Create number buttons
        float buttonsStartY = totalHeight - padding - displayHeight - padding;
        CreateNumberGrid(padding, buttonsStartY);
        
        // Create FILL button at bottom
        float fillY = padding;
        CreateFillButton(padding, fillY, gridWidth, fillButtonHeight);
        
        keypadPanel.SetActive(false);
    }

    void CreateKeypadDisplay(float xOffset, float topY)
    {
        GameObject displayBg = new GameObject("DisplayBg");
        displayBg.transform.SetParent(keypadPanel.transform, false);
        
        RectTransform bgRect = displayBg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 1);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.pivot = new Vector2(0.5f, 1);
        bgRect.anchoredPosition = new Vector2(0, -15);
        bgRect.sizeDelta = new Vector2(-30, 50);
        
        Image bgImage = displayBg.AddComponent<Image>();
        bgImage.color = displayBgColor;
        
        GameObject displayObj = new GameObject("DisplayText");
        displayObj.transform.SetParent(displayBg.transform, false);
        
        RectTransform displayRect = displayObj.AddComponent<RectTransform>();
        displayRect.anchorMin = Vector2.zero;
        displayRect.anchorMax = Vector2.one;
        displayRect.offsetMin = new Vector2(10, 5);
        displayRect.offsetMax = new Vector2(-10, -5);
        
        keypadDisplayText = displayObj.AddComponent<TextMeshProUGUI>();
        keypadDisplayText.text = "";
        keypadDisplayText.fontSize = 28;
        keypadDisplayText.fontStyle = FontStyles.Bold;
        keypadDisplayText.color = displayTextColor;
        keypadDisplayText.alignment = TextAlignmentOptions.Center;
        
        UpdateKeypadDisplay();
    }

    void CreateNumberGrid(float xOffset, float topY)
    {
        float startX = buttonSize / 2 + 15;
        float startY = -80;
        
        // Row 1: 1, 2, 3
        CreateNumButton("1", startX, startY);
        CreateNumButton("2", startX + buttonSize + buttonSpacing, startY);
        CreateNumButton("3", startX + 2 * (buttonSize + buttonSpacing), startY);
        
        // Row 2: 4, 5, 6
        float y2 = startY - buttonSize - buttonSpacing;
        CreateNumButton("4", startX, y2);
        CreateNumButton("5", startX + buttonSize + buttonSpacing, y2);
        CreateNumButton("6", startX + 2 * (buttonSize + buttonSpacing), y2);
        
        // Row 3: 7, 8, 9
        float y3 = y2 - buttonSize - buttonSpacing;
        CreateNumButton("7", startX, y3);
        CreateNumButton("8", startX + buttonSize + buttonSpacing, y3);
        CreateNumButton("9", startX + 2 * (buttonSize + buttonSpacing), y3);
        
        // Row 4: C, 0, ⌫
        float y4 = y3 - buttonSize - buttonSpacing;
        CreateActionBtn("C", startX, y4, clearButtonColor, ClearKeypad);
        CreateNumButton("0", startX + buttonSize + buttonSpacing, y4);
        CreateActionBtn("⌫", startX + 2 * (buttonSize + buttonSpacing), y4, deleteButtonColor, RemoveLastDigit);
    }

    void CreateNumButton(string digit, float x, float y)
    {
        GameObject btnObj = new GameObject($"Num_{digit}");
        btnObj.transform.SetParent(keypadPanel.transform, false);
        
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0, 1);
        btnRect.anchorMax = new Vector2(0, 1);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(x, y);
        btnRect.sizeDelta = new Vector2(buttonSize, buttonSize);
        
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = numberButtonColor;
        
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.3f, 1.3f, 1.3f, 1f);
        colors.pressedColor = new Color(1.5f, 1.5f, 1.5f, 1f);
        btn.colors = colors;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TMP_Text btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = digit;
        btnText.fontSize = 28;
        btnText.fontStyle = FontStyles.Bold;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
        
        char digitChar = digit[0];
        btn.onClick.AddListener(() => AddDigit(digitChar));
    }

    void CreateActionBtn(string label, float x, float y, Color color, System.Action action)
    {
        GameObject btnObj = new GameObject($"Action_{label}");
        btnObj.transform.SetParent(keypadPanel.transform, false);
        
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0, 1);
        btnRect.anchorMax = new Vector2(0, 1);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(x, y);
        btnRect.sizeDelta = new Vector2(buttonSize, buttonSize);
        
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = color;
        
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.3f, 1.3f, 1.3f, 1f);
        colors.pressedColor = new Color(1.5f, 1.5f, 1.5f, 1f);
        btn.colors = colors;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TMP_Text btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = label;
        btnText.fontSize = label == "⌫" ? 26 : 22;
        btnText.fontStyle = FontStyles.Bold;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
        
        btn.onClick.AddListener(() => action());
    }

    void CreateFillButton(float x, float y, float width, float height)
    {
        GameObject btnObj = new GameObject("FillButton");
        btnObj.transform.SetParent(keypadPanel.transform, false);
        
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0, 0);
        btnRect.anchorMax = new Vector2(1, 0);
        btnRect.pivot = new Vector2(0.5f, 0);
        btnRect.anchoredPosition = new Vector2(0, 15);
        btnRect.sizeDelta = new Vector2(-30, height);
        
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = fillButtonColor;
        
        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
        colors.pressedColor = new Color(1.4f, 1.4f, 1.4f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(FillInputAndClose);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TMP_Text btnText = textObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "FILL ✓";
        btnText.fontSize = 20;
        btnText.fontStyle = FontStyles.Bold;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
    }

    void ToggleKeypad()
    {
        if (keypadVisible)
            HideKeypad();
        else
            ShowKeypad();
    }

    void ShowKeypad()
    {
        if (keypadPanel != null)
        {
            keypadPanel.SetActive(true);
            keypadVisible = true;
            keypadText = codeInputField != null ? codeInputField.text : "";
            UpdateKeypadDisplay();
        }
    }

    void HideKeypad()
    {
        if (keypadPanel != null)
        {
            keypadPanel.SetActive(false);
            keypadVisible = false;
        }
    }

    void FillInputAndClose()
    {
        if (codeInputField != null)
            codeInputField.text = keypadText;
        
        HideKeypad();
        
        if (feedbackText != null)
            feedbackText.text = "";
    }

    void AddDigit(char digit)
    {
        if (keypadText.Length >= MAX_CODE_LENGTH) return;
        keypadText += digit;
        UpdateKeypadDisplay();
    }

    void RemoveLastDigit()
    {
        if (keypadText.Length > 0)
        {
            keypadText = keypadText.Substring(0, keypadText.Length - 1);
            UpdateKeypadDisplay();
        }
    }

    void ClearKeypad()
    {
        keypadText = "";
        UpdateKeypadDisplay();
    }

    void UpdateKeypadDisplay()
    {
        if (keypadDisplayText == null) return;
        
        if (string.IsNullOrEmpty(keypadText))
        {
            keypadDisplayText.text = "_ _ _ _ _ _";
            keypadDisplayText.color = new Color(displayTextColor.r, displayTextColor.g, displayTextColor.b, 0.5f);
        }
        else
        {
            keypadDisplayText.text = string.Join(" ", keypadText.ToCharArray());
            keypadDisplayText.color = displayTextColor;
        }
    }

    void ShowFeedback(string message, bool isError)
    {
        if (feedbackText == null) return;
        feedbackText.text = message;
        feedbackText.color = isError ? new Color(0.95f, 0.3f, 0.3f, 1f) : new Color(0.3f, 0.85f, 0.45f, 1f);
        feedbackText.gameObject.SetActive(true);
    }

    void TryAccess()
    {
        if (isProcessingAccess) return;
        
        string code = "";
        if (codeInputField != null)
            code = codeInputField.text.Trim().ToUpper();
        
        if (string.IsNullOrEmpty(code))
        {
            ShowFeedback("Please enter a code", true);
            return;
        }
        
        Debug.Log($"TicketAccessGate: Checking code '{code}' against '{correctCode}'");
        
        if (code == correctCode.ToUpper())
        {
            ShowFeedback("ACCESS GRANTED!", false);
            isProcessingAccess = true;
            
            // Hide keypad
            HideKeypad();
            
            // Use Invoke instead of Coroutine to avoid "object inactive" error
            Invoke("LoadTargetScene", 1.0f);
        }
        else
        {
            ShowFeedback("Invalid code - try again", true);
            if (codeInputField != null)
                codeInputField.text = "";
            keypadText = "";
            UpdateKeypadDisplay();
        }
    }

    void LoadTargetScene()
    {
        Debug.Log($"TicketAccessGate: LoadTargetScene called, index={targetSceneIndex}");
        
        // Disable gate blocker AFTER showing message
        if (gateBlocker != null)
            gateBlocker.SetActive(false);
        
        SetPlayerUIState(false);
        
        if (targetSceneIndex >= 0 && targetSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"TicketAccessGate: Loading scene {targetSceneIndex}");
            SceneManager.LoadScene(targetSceneIndex);
        }
        else
        {
            Debug.LogError($"TicketAccessGate: Invalid scene index {targetSceneIndex}");
            isProcessingAccess = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("TicketAccessGate: Player entered trigger");
            
            playerNearby = true;
            FindPlayerControllerNow();
            
            // Ensure EventSystem is working
            EnsureEventSystem();
            
            // Fix popup size before showing
            FixPopupSize();
            
            if (popupPanel != null)
            {
                popupPanel.SetActive(true);
                if (codeInputField != null)
                    codeInputField.text = "";
                keypadText = "";
                if (feedbackText != null)
                    feedbackText.text = "";
            }
            
            // Ensure keypad button exists
            if (openKeypadButton == null)
            {
                Debug.Log("TicketAccessGate: Recreating keypad button");
                CreateOpenKeypadButton();
            }
            
            // Ensure keypad exists
            if (keypadPanel == null)
            {
                Debug.Log("TicketAccessGate: Recreating keypad");
                CreateKeypadWithDisplay();
            }
            
            HideKeypad();
            SetPlayerUIState(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            
            if (popupPanel != null)
                popupPanel.SetActive(false);
            
            HideKeypad();
            keypadText = "";
            isProcessingAccess = false;
            
            SetPlayerUIState(false);
        }
    }

    void FindPlayerController()
    {
        if (playerController == null)
            playerController = FindObjectOfType<FirstPersonController>();
    }

    void FindPlayerControllerNow()
    {
        playerController = FindObjectOfType<FirstPersonController>();
    }

    void FindSubmitButton()
    {
        if (submitButton == null && popupPanel != null)
            submitButton = popupPanel.GetComponentInChildren<Button>();
    }

    void FindJoystick()
    {
        joystickToDisable = FindObjectOfType<Joystick>();
        if (joystickToDisable != null)
        {
            joystickCanvasGroup = joystickToDisable.GetComponent<CanvasGroup>();
            if (joystickCanvasGroup == null)
                joystickCanvasGroup = joystickToDisable.gameObject.AddComponent<CanvasGroup>();
        }
    }

    void SetPlayerUIState(bool isOpen)
    {
        if (playerController != null)
            playerController.SetUIOpen(isOpen);
        
        if (joystickCanvasGroup != null)
        {
            joystickCanvasGroup.interactable = !isOpen;
            joystickCanvasGroup.blocksRaycasts = !isOpen;
        }
    }
}
