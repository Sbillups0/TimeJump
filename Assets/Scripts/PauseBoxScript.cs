using UnityEngine;

public class TutorialBox : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [TextArea(3, 10)]
    [SerializeField] private string tutorialMessage = "Tutorial text goes here";

    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMPro.TextMeshProUGUI textField;

    private bool isShowing = false;

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

        Time.timeScale = 0f;
        isShowing = true;
    }

    void Update()
    {
        if (isShowing && Input.GetMouseButtonDown(0))
            Dismiss();
    }

    public void Dismiss()
    {
        Time.timeScale = 1f;
        isShowing = false;
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }
}