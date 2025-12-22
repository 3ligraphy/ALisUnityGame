using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SlideShowController : MonoBehaviour
{
    [Header("UI References")]
    public Image slideImage;
    public Text slideText;
    public Button nextButton;
    public Button startButton;
    public CanvasGroup canvasGroup;

    [Header("Fade Panel")]
    public Image blackScreenImage;

    [Header("Cover (First Screen)")]
    public Sprite coverImage;     // صورة الكفر
    [TextArea] public string coverText; // نص الكفر
    public float coverDisplayTime = 3f; // مدة عرض الكفر

    [Header("Slides Content")]
    public Sprite[] slideImages;
    [TextArea] public string[] slideTexts;

    [Header("Settings")]
    public float fadeDuration = 0.5f;

    private int index = -1;  // نبدأ -1 لأن أول عرض هو الكفر وليس من السلايد شو
    private bool coverFinished = false;

    void Start()
    {
        startButton.gameObject.SetActive(false);
        nextButton.onClick.AddListener(NextSlide);
        startButton.onClick.AddListener(StartGame);

        // تشغيل البداية
        StartCoroutine(ShowCoverThenSlides());
    }

    IEnumerator ShowCoverThenSlides()
    {
        // 1. عرض الكفر
        slideImage.sprite = coverImage;
        slideText.text = coverText;
        canvasGroup.alpha = 1;

        // 2. Fade للشاشة السوداء لو موجودة
        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(true);
            Color startColor = blackScreenImage.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                blackScreenImage.color = Color.Lerp(startColor, endColor, t / fadeDuration);
                yield return null;
            }

            blackScreenImage.color = endColor;
            blackScreenImage.gameObject.SetActive(false);
        }

        // 3. الانتظار لمدة عرض الكفر
        yield return new WaitForSeconds(coverDisplayTime);

        // السماح ببدء السلايد شو
        coverFinished = true;

        // أول شريحة
        NextSlide();
    }

    void UpdateSlide()
    {
        slideImage.sprite = slideImages[index];
        slideText.text = slideTexts[index];
    }

    public void NextSlide()
    {
        if (!coverFinished) return;

        // انتقال السلايد
        if (index < slideImages.Length - 1)
        {
            index++;
            StartCoroutine(FadeSlide());
        }

        // آخر شريحة
        if (index == slideImages.Length - 1)
        {
            nextButton.gameObject.SetActive(false);
            startButton.gameObject.SetActive(true);
        }
    }

    IEnumerator FadeSlide()
    {
        // Fade Out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0;

        // تحديث المحتوى
        UpdateSlide();

        // Fade In
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1;
    }

    void StartGame()
    {
        SceneManager.LoadScene("Map");
    }
}
