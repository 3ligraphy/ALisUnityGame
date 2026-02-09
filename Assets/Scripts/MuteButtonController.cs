using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Adds a persistent mute button on screen. Toggles all game audio (AudioListener).
/// </summary>
public class MuteButtonController : MonoBehaviour
{
    [Header("Button Settings")]
    public float buttonSize = 56f;
    public float marginFromEdge = 24f;
    public Color buttonColor = new Color(0.25f, 0.25f, 0.3f, 0.9f);
    public Color buttonColorMuted = new Color(0.6f, 0.2f, 0.2f, 0.9f);

    private static GameObject canvasObj;
    private static MuteButtonController instance;
    private Button muteButton;
    private Image buttonImage;
    private TextMeshProUGUI labelText;
    private bool isMuted;
    private const string PrefsKey = "GameMuted";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureInstance()
    {
        if (instance != null) return;
        var go = new GameObject("MuteButtonController");
        instance = go.AddComponent<MuteButtonController>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        isMuted = PlayerPrefs.GetInt(PrefsKey, 0) == 1;
        ApplyMuteState();
    }

    void Start()
    {
        CreateButtonIfNeeded();
    }

    void CreateButtonIfNeeded()
    {
        if (canvasObj != null && muteButton != null) return;

        // Reuse or create canvas
        canvasObj = GameObject.Find("MuteButtonCanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("MuteButtonCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 502;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasObj);
        }

        Transform parent = canvasObj.transform;
        Transform existing = parent.Find("MuteButton");
        if (existing != null)
        {
            muteButton = existing.GetComponent<Button>();
            buttonImage = existing.GetComponent<Image>();
            labelText = existing.GetComponentInChildren<TextMeshProUGUI>();
            if (muteButton != null) muteButton.onClick.AddListener(OnMuteClicked);
            UpdateButtonAppearance();
            return;
        }

        GameObject btnObj = new GameObject("MuteButton");
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.sizeDelta = new Vector2(buttonSize, buttonSize);
        rect.anchoredPosition = new Vector2(-marginFromEdge, -marginFromEdge);

        buttonImage = btnObj.AddComponent<Image>();
        buttonImage.color = isMuted ? buttonColorMuted : buttonColor;

        muteButton = btnObj.AddComponent<Button>();
        muteButton.onClick.AddListener(OnMuteClicked);

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.fontSize = 28;
        labelText.raycastTarget = false;

        UpdateButtonAppearance();
    }

    void OnMuteClicked()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt(PrefsKey, isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMuteState();
        UpdateButtonAppearance();
    }

    void ApplyMuteState()
    {
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (listener != null)
            listener.enabled = !isMuted;
    }

    void UpdateButtonAppearance()
    {
        if (buttonImage != null)
            buttonImage.color = isMuted ? buttonColorMuted : buttonColor;
        if (labelText != null)
            labelText.text = isMuted ? "🔇" : "🔊";
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMuteState();
    }
}
