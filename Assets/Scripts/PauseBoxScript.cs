using UnityEngine;

public class TutorialBox : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [TextArea(3, 10)]
    [SerializeField] private string tutorialMessage = "Tutorial text goes here";

    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMPro.TextMeshProUGUI textField;

    void Start()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    public void Show()
    {
        if (textField != null)
            textField.text = tutorialMessage;

        if (popupRoot != null)
            popupRoot.SetActive(true);
       
    }

    public void Hide()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }
}