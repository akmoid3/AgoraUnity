using TMPro;
using UnityEngine;
using System.Collections;
public class Popup : MonoBehaviour
{
    [SerializeField] private GameObject scrollView;
    [SerializeField] private TMP_Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scrollView.SetActive(false);
    }

    public void ShowMessage(string message, float duration = 5f)
    {
        text.text = message;
        scrollView.SetActive(true);
        StartCoroutine(HideAfterSeconds(duration));
    }

    private IEnumerator HideAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        scrollView.SetActive(false);
    }

}
