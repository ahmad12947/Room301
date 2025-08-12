using UnityEngine;
using UnityEngine.UI; // Needed for Button

public class OpenWebpageOnClick : MonoBehaviour
{
    [Header("Webpage to Open")]
    [Tooltip("The URL that will open when clicking the button.")]
    public string webpageURL = "https://www.example.com";

    private void Start()
    {
        // Try to get the Button component
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OpenWebpage);
        }
        else
        {
            Debug.LogWarning("No Button component found on this GameObject.");
        }
    }

    public void OpenWebpage()
    {
        if (!string.IsNullOrEmpty(webpageURL))
        {
            Application.OpenURL(webpageURL);
        }
        else
        {
            Debug.LogWarning("Webpage URL is empty.");
        }
    }
}
