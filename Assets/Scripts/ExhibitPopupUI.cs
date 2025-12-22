using UnityEngine;
using TMPro;
using UnityEngine.UI; // عشان نستخدم Image

public class ExhibitPopupUI : MonoBehaviour
{
    public GameObject popupPanel;
    public TMP_Text exhibitNameText;
    public TMP_Text exhibitInfoText;
    public Image exhibitImage; // الصورة اللي هتظهر

    void Start()
    {
        popupPanel.SetActive(false);
    }

    public void ShowPopup(string name, string info, Sprite image)
    {
        exhibitNameText.text = name;
        exhibitInfoText.text = info;

        if (exhibitImage != null && image != null)
        {
            exhibitImage.sprite = image;
            exhibitImage.gameObject.SetActive(true);
        }
        else if (exhibitImage != null)
        {
            exhibitImage.gameObject.SetActive(false);
        }

        popupPanel.SetActive(true);
    }

    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }
}
