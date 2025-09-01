using TMPro;
using UnityEngine;
using System.Collections;

public class Popup : MonoBehaviour
{
    [SerializeField] private GameObject textPrefab; 
    [SerializeField] private RectTransform textParent;  

    private int maxCharsPerLine = 18;
    private int maxLines = 3;

    public void ShowMessage(string message, float duration = 5f)
    {
        GameObject newTextGO = Instantiate(textPrefab, textParent);
        TMP_Text tmp = newTextGO.GetComponent<TMP_Text>();

        if (tmp == null)
        {
            Debug.LogWarning("TMP_Text NULLLLLLLLLL!!!!");
            return;
        }

        string formatted = InsertLineBreaks(message, maxCharsPerLine);
        string[] lines = formatted.Split('\n');

        if (lines.Length > maxLines)
        {
            formatted = string.Join("\n", lines, 0, maxLines);
            formatted += "...";
        }

        tmp.text = formatted;

        Canvas.ForceUpdateCanvases();
        RecenterParent();

        Destroy(newTextGO, duration);
    }

    private string InsertLineBreaks(string input, int maxChars)
    {
        if (string.IsNullOrEmpty(input)) return input;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            sb.Append(input[i]);
            if ((i + 1) % maxChars == 0)
                sb.Append('\n');
        }
        return sb.ToString();
    }

    private void RecenterParent()
    {
        if (textParent == null) return;

        float parentHeight = (textParent.parent as RectTransform).rect.height;
        float contentHeight = textParent.rect.height;

        if (contentHeight < parentHeight)
        {
            textParent.pivot = new Vector2(0.5f, 0.5f);
            textParent.anchoredPosition = new Vector2(0.0f, -40.0f);
        }
        else
        {
            textParent.pivot = new Vector2(0.5f, 1f);
            textParent.anchoredPosition = new Vector2(0.0f, -40.0f);
        }
    }
}
