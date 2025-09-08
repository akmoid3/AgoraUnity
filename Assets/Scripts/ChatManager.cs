using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public static ChatManager instance;
    [SerializeField] private Transform chatContent;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private AudioClip messageSound;

    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        if(instance == null)
            instance = this;
        else
        {
            Destroy(this);
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void CreateMessage(string username, string message)
    {
        GameObject newMessage = Instantiate(messagePrefab, chatContent);

        TMP_Text tmpText = newMessage.GetComponentInChildren<TMP_Text>();
        tmpText.text = username + " : " + message;

        LayoutRebuilder.ForceRebuildLayoutImmediate(chatContent as RectTransform);

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        
        if(audioSource != null)
            audioSource.PlayOneShot(messageSound);

        var matchingUsers = UserManager.instance.Users
            .Where(user => user.Anchor != null && user.RtmID == username)
            .ToList();

        var userFound = UserManager.instance.Users
            .FirstOrDefault(user => user.Anchor != null && user.RtmID == username);
        
        
        if(userFound != null)
        {
            Debug.Log("Utente trovato: " + userFound.RtmID);
            
            Popup popup = userFound.Anchor.GetComponent<Popup>();

            if (popup != null)
            {
                popup.ShowMessage(message);
            }
        }

    }
}
