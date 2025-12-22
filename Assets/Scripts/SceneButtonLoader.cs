using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneButtonLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    public int targetSceneIndex = 1;   // رقم المشهد اللي هتحدده من الـ Inspector

    [Header("UI Button")]
    public Button loadSceneButton;     // الزر اللي هيعمل الانتقال

    void Start()
    {
        // ربط الزر بالدالة
        if (loadSceneButton != null)
            loadSceneButton.onClick.AddListener(LoadTargetScene);
        else
            Debug.LogWarning("⚠️ لم يتم ربط الزر في Inspector!");
    }

    void LoadTargetScene()
    {
        SceneManager.LoadScene(targetSceneIndex);
    }
}
