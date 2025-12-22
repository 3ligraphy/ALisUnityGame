using UnityEngine;
using UnityEngine.UI;

// تم تغيير اسم الكلاس هنا
public class WindowController : MonoBehaviour
{
    [Header("UI References")]
    // المتغير الذي سنربطه باللوحة (Panel) التي تمثل البوب-أب
    public GameObject popupPanel; 

    void Start()
    {
        // تأكد من أن البوب-أب مخفي عند بداية تشغيل المشهد
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    /// <summary>
    /// دالة لفتح وإظهار النافذة المنبثقة.
    /// </summary>
    public void OpenPopup()
    {
        if (popupPanel != null)
        {
            // إظهار النافذة المنبثقة
            popupPanel.SetActive(true);
            Debug.Log("Popup opened.");
        }
    }

    /// <summary>
    /// دالة لإغلاق وإخفاء النافذة المنبثقة.
    /// </summary>
    public void ClosePopup()
    {
        if (popupPanel != null)
        {
            // إخفاء النافذة المنبثقة
            popupPanel.SetActive(false);
            Debug.Log("Popup closed.");
        }
    }
}