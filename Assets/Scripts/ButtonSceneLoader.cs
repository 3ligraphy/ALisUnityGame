using UnityEngine;
using UnityEngine.SceneManagement; // يجب تضمين هذه المكتبة لتحميل المشاهد

public class ButtonSceneLoader : MonoBehaviour
{
    // [SerializeField] يجعل المتغير يظهر في لوحة Inspector بالرغم من كونه private
    // ولكن يفضل استخدام public في هذه الحالة لسهولة الوصول من الزر
    [Tooltip("أدخل فهرس المشهد (Scene Index) كما هو محدد في Build Settings.")]
    public int targetSceneIndex = 0; // القيمة الافتراضية هي 0 (أول مشهد)

    /// <summary>
    /// يتم استدعاء هذه الدالة عند الضغط على الزر.
    /// </summary>
    public void LoadTargetScene()
    {
        // التحقق من أن فهرس المشهد ضمن النطاق المسموح به
        if (targetSceneIndex >= 0 && targetSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // تحميل المشهد باستخدام فهرسه (الرقم)
            SceneManager.LoadScene(targetSceneIndex);
        }
        else
        {
            Debug.LogError("فهرس المشهد المطلوب (" + targetSceneIndex + ") غير صالح أو غير موجود في Build Settings!");
        }
    }
}