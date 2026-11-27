using UnityEngine;
using UnityEngine.UI;

public static class GameMessageUI
{
    private static Text cachedText;

    private static Text GetText()
    {
        if (cachedText == null)
        {
            GameObject namedObj = GameObject.Find("StatusText");
            if (namedObj != null)
            {
                cachedText = namedObj.GetComponent<Text>();
            }

            if (cachedText == null)
            {
                cachedText = Object.FindObjectOfType<Text>();
            }
        }

        return cachedText;
    }

    public static void RegisterText(Text explicitText)
    {
        if (explicitText != null)
        {
            cachedText = explicitText;
        }
    }

    public static void ShowMessage(string message)
    {
        Text uiText = GetText();
        if (uiText != null)
        {
            uiText.gameObject.SetActive(true);
            uiText.text = message;
        }
    }

    public static void ClearMessage()
    {
        Text uiText = GetText();
        if (uiText != null)
        {
            uiText.gameObject.SetActive(true);
            uiText.text = "";
        }
    }
}
