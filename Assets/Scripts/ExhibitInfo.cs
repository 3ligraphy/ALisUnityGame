using UnityEngine;

public class ExhibitInfo : MonoBehaviour
{
    [Header("Exhibit Data")]
    public string exhibitName;
    [TextArea] public string exhibitInfo;
    public Sprite exhibitSprite; // الصورة الخاصة بالتمثال

    private ExhibitPopupUI popupUI;

    void Start()
    {
        popupUI = FindObjectOfType<ExhibitPopupUI>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            popupUI.ShowPopup(exhibitName, exhibitInfo, exhibitSprite);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            popupUI.HidePopup();
        }
    }
}
