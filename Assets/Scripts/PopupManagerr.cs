using UnityEngine;

public class PopupManagerr : MonoBehaviour
{
    // 🔥 هذا المتغير هو GameObject الخاص بالنافذة المنبثقة (Pop-up Panel)
    [Tooltip("اسحب عنصر الـ UI (Panel) الذي يمثل النافذة المنبثقة هنا")]
    public GameObject popupPanel;

    // لغرض التجربة: يمكنك إضافة نص لتغييره
    // public TMPro.TextMeshProUGUI popupText;

    void Start()
    {
        // تأكد من أن البوب-أب مخفي عند بداية اللعبة
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    /// <summary>
    /// دالة يتم استدعاؤها لإظهار النافذة المنبثقة.
    /// هذه هي الدالة التي سيتم ربطها في الـ Inspector للـ Cube.
    /// </summary>
    public void ShowPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            Debug.Log("تم إظهار البوب-أب بنجاح!");

            // (اختياري) قفل حركة اللاعب عند ظهور البوب-أب
            // يمكننا الحصول على مرجع لـ FirstPersonController وتعطيله هنا
            // FindObjectOfType<FirstPersonController>().enabled = false; 
        }
    }

    /// <summary>
    /// دالة يتم استدعاؤها لإخفاء النافذة المنبثقة (مثلاً عند النقر على زر إغلاق).
    /// </summary>
    public void HidePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
            Debug.Log("تم إخفاء البوب-أب.");

            // (اختياري) إعادة تفعيل حركة اللاعب
            // FindObjectOfType<FirstPersonController>().enabled = true;
        }
    }
}