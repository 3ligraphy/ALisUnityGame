using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Text questionText;            // النص اللي فيه السؤال
    public ArabicText feedbackArabic;    // سكربت ArabicText مربوط على Text النتيجة
    public Button yesButton;             // زر نعم
    public Button noButton;              // زر لا

    [Header("Question Settings")]
    public string question = "هل السماء زرقاء؟"; // السؤال
    public bool correctAnswer = true;             // الإجابة الصحيحة (true = نعم, false = لا)

    void Start()
    {
        // عرض السؤال
        questionText.text = question;
        feedbackArabic.Text = ""; // تفريغ النتيجة في البداية

        // ربط الأزرار بالدوال
        yesButton.onClick.AddListener(() => CheckAnswer(true));
        noButton.onClick.AddListener(() => CheckAnswer(false));
    }

    void CheckAnswer(bool playerAnswer)
    {
        if (playerAnswer == correctAnswer)
        {
            feedbackArabic.Text = "أحسنت";
            yesButton.gameObject.SetActive(false);
            noButton.gameObject.SetActive(false);
        }
        else
        {
            feedbackArabic.Text = "الإجابة خاطئة";
        }

        // إخفاء نص السؤال بعد الإجابة
        questionText.gameObject.SetActive(false);

        // تحديث النص العربي بعد تغييره
        feedbackArabic.FixTextForUI();
    }
}
