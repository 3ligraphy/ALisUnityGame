using UnityEngine;
using UnityEngine.UI;

public class CubeClickHandler : MonoBehaviour
{
    // المتغيرات العامة
    public GameObject popupPanel; // نافذة UI التي تحتوي على الصورة والزر
    public Button closeButton;    // زر الإغلاق
    public float maxInteractionDistance = 3.0f; // أقصى مسافة للضغط على المكعب

    // متغير خاص لتخزين مرجع للاعب
    private Transform playerTransform;

    void Start()
    {
        // 1. البحث عن اللاعب وتخزين مرجعه في البداية
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player GameObject not found! Make sure your Player has the tag 'Player'.");
        }

        // 2. تأكد أن الزر مربوط بوظيفة الإغلاق
        closeButton.onClick.AddListener(HidePopup);

        // 3. تأكد أن البوب أب مغلق في البداية
        popupPanel.SetActive(false);
    }

    // تُستدعى عند الضغط على المكعب بالماوس
    void OnMouseDown()
    {
        // **الشرط الأول: منع الفتح إذا كان هناك بوب أب آخر مفتوح بالفعل**
        // نتحقق من حالته قبل أي شيء.
        if (popupPanel.activeInHierarchy)
        {
            // إذا كان البوب أب الخاص بهذا المكعب مفتوحاً، نتجاهل الضغطة.
            return;
        }

        // **الشرط الثاني: التحقق من المسافة**
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            if (distance <= maxInteractionDistance)
            {
                // إذا كانت المسافة ضمن الحد المسموح به، نفعل البوب أب
                ShowPopup();

                // **التحكم في التفاعل مع المكعبات الأخرى (إغلاق كل المكعبات الأخرى)**
                // يجب أن تكون هناك آلية لتعطيل تفاعل المكعبات الأخرى.
                // أفضل طريقة هي استخدام مدير (Manager) في المشهد (Scene)
                // يخبر المكعبات الأخرى بعدم السماح بالضغط.
                // لتبسيط الأمر هنا، سنفترض أننا نستخدم مدير يتحقق من وجود بوب أب مفتوح.
                
                // للحصول على المنع الشامل لجميع المكعبات الأخرى:
                // يجب تطبيق آلية إيقاف التفاعل العام (مثل متغير ثابت أو خدمة في Manager).
                // في هذا الكود، سنركز على الشروط المحلية (المسافة ومنع إعادة الفتح لنفس المكعب)
                // وافتراض أنك ستستخدم مدير للتحكم في حالة الفتح العامة.
                
                // **ملاحظة:** لتحقيق منع الضغط على أي كيوب آخر، يجب عليك إضافة
                // شرط في بداية OnMouseDown يتحقق من متغير ثابت/عام في Scene Manager
                // يشير إلى ما إذا كان هناك "Popup" مفتوح حالياً.
            }
            else
            {
                Debug.Log("Too far from the cube! Distance: " + distance + ". Required: " + maxInteractionDistance);
            }
        }
    }

    void ShowPopup()
    {
        popupPanel.SetActive(true);
        // **هنا يجب استدعاء دالة في Manager لتسجيل أن هناك بوب أب مفتوح الآن**
        // ManagerScript.Instance.IsPopupOpen = true; 
    }

    void HidePopup()
    {
        popupPanel.SetActive(false);
        // **هنا يجب استدعاء دالة في Manager لتسجيل أن البوب أب أغلق**
        // ManagerScript.Instance.IsPopupOpen = false; 
    }
}