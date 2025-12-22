using UnityEngine;
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

    private bool playerNearby = false;

    void Start()
    {
        popupPanel.SetActive(false);
        feedbackText.text = "";
    }

    void Update()
    {
        // لو اللاعب قريب وضغط Enter
        if (playerNearby && Input.GetKeyDown(KeyCode.Return))
        {
            TryAccess();
        }
    }

    public void TryAccess()
    {
        string enteredCode = codeInputField.text.Trim();

        if (enteredCode.Equals(correctCode, System.StringComparison.OrdinalIgnoreCase))
        {
            feedbackText.text = "✅ Verified! Welcome.";
            popupPanel.SetActive(false);

            if (gateBlocker != null)
                gateBlocker.SetActive(false); // يفتح الباب أو يزيل الحاجز

            // الانتقال إلى المشهد المحدد
            SceneManager.LoadScene(targetSceneIndex);
        }
        else
        {
            feedbackText.text = "❌ Invalid code. Try again.";
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            popupPanel.SetActive(true);
            feedbackText.text = "";
            codeInputField.text = "";
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
