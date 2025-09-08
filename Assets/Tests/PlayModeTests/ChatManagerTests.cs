using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

[TestFixture]
public class ChatManagerTests
{
    private ChatManager chatManager;
    private GameObject testGameObject;
    private Transform chatContent;
    private GameObject mockMessagePrefab;
    private ScrollRect scrollRect;
    private AudioClip mockAudioClip;
    private AudioSource audioSource;
    private Canvas testCanvas;
    private List<GameObject> createdObjects;

    [SetUp]
    public void SetUp()
    {
        createdObjects = new List<GameObject>();
        LogAssert.ignoreFailingMessages = true;
        
        CreateTestCanvas();
        CreateChatManager();
        CreateUIComponents();
        SetupUserManagerMock();
        SetupChatManagerFields();
        InitializeChatManager();
        
        LogAssert.ignoreFailingMessages = false;
    }

    [TearDown]
    public void TearDown()
    {
        LogAssert.ignoreFailingMessages = true;
        
        ChatManager.instance = null;
        if (UserManager.instance != null && UserManager.instance.gameObject != null)
        {
            Object.DestroyImmediate(UserManager.instance.gameObject);
        }
        UserManager.instance = null;
        
        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            if (createdObjects[i] != null)
            {
                Object.DestroyImmediate(createdObjects[i]);
            }
        }
        createdObjects.Clear();
        
        chatManager = null;
        testGameObject = null;
        chatContent = null;
        mockMessagePrefab = null;
        scrollRect = null;
        audioSource = null;
        testCanvas = null;
        
        LogAssert.ignoreFailingMessages = false;
    }

    private void CreateTestCanvas()
    {
        var canvasGO = new GameObject("TestCanvas");
        testCanvas = canvasGO.AddComponent<Canvas>();
        testCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        createdObjects.Add(canvasGO);
    }

    private void CreateChatManager()
    {
        testGameObject = new GameObject("ChatManagerTest");
        chatManager = testGameObject.AddComponent<ChatManager>();
        audioSource = testGameObject.AddComponent<AudioSource>();
        createdObjects.Add(testGameObject);
    }

    private void CreateUIComponents()
    {
        var scrollRectGO = new GameObject("ScrollRect", typeof(RectTransform));
        scrollRectGO.transform.SetParent(testCanvas.transform, false);
        scrollRect = scrollRectGO.AddComponent<ScrollRect>();
        scrollRectGO.AddComponent<Image>().color = Color.clear; 
        
        var scrollRectTransform = scrollRectGO.GetComponent<RectTransform>();
        scrollRectTransform.sizeDelta = new Vector2(400, 300);
        scrollRectTransform.anchoredPosition = Vector2.zero;
        createdObjects.Add(scrollRectGO);

        var viewportGO = new GameObject("Viewport", typeof(RectTransform));
        viewportGO.transform.SetParent(scrollRectGO.transform, false);
        
        var viewportRect = viewportGO.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        
        var mask = viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        var image = viewportGO.AddComponent<Image>();
        image.color = Color.clear;
        
        createdObjects.Add(viewportGO);

        var contentGO = new GameObject("Content", typeof(RectTransform)); // Specifica RectTransform nel costruttore
        contentGO.transform.SetParent(viewportGO.transform, false);
        chatContent = contentGO.transform;
        
        var contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 100);
        contentRect.anchoredPosition = Vector2.zero;

        var layoutGroup = contentGO.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;

        var sizeFitter = contentGO.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        createdObjects.Add(contentGO);

        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        CreateMessagePrefab();
        
        mockAudioClip = AudioClip.Create("MockClip", 44100, 1, 44100, false);
    }

    private void CreateMessagePrefab()
    {
        mockMessagePrefab = new GameObject("MockMessagePrefab", typeof(RectTransform));
        
        var prefabRect = mockMessagePrefab.GetComponent<RectTransform>();
        prefabRect.sizeDelta = new Vector2(350, 30);

        var layoutElement = mockMessagePrefab.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 30;

        var tmpText = mockMessagePrefab.AddComponent<TextMeshProUGUI>();
        tmpText.text = "";
        tmpText.fontSize = 14;
        tmpText.color = Color.white;
        tmpText.alignment = TextAlignmentOptions.Left;

        createdObjects.Add(mockMessagePrefab);
    }

    private void SetupUserManagerMock()
    {
        if (UserManager.instance == null)
        {
            var userManagerGO = new GameObject("MockUserManager");
            var userManager = userManagerGO.AddComponent<UserManager>();
            UserManager.instance = userManager;
            
            var usersField = typeof(UserManager).GetField("users", BindingFlags.NonPublic | BindingFlags.Instance);
            usersField?.SetValue(userManager, new List<User>());
            
            createdObjects.Add(userManagerGO);
        }
    }

    private void SetupChatManagerFields()
    {
        SetPrivateField("chatContent", chatContent);
        SetPrivateField("messagePrefab", mockMessagePrefab);
        SetPrivateField("scrollRect", scrollRect);
        SetPrivateField("messageSound", mockAudioClip);
    }

    private void InitializeChatManager()
    {
        ChatManager.instance = null;
        chatManager.Start();
    }

    [Test]
    public void Start_ShouldSetSingletonInstance()
    {
        Assert.AreEqual(chatManager, ChatManager.instance, "Should set singleton instance");
    }

    [Test]
    public void CreateMessage_WithValidInputs_ShouldCreateMessageObject()
    {
        string username = "TestUser";
        string message = "Test message";
        int initialChildCount = chatContent.childCount;
        
        chatManager.CreateMessage(username, message);
        
        Assert.AreEqual(initialChildCount + 1, chatContent.childCount, "Should create one message object");
        
        var createdMessage = chatContent.GetChild(chatContent.childCount - 1);
        var tmpText = createdMessage.GetComponentInChildren<TMP_Text>();
        Assert.IsNotNull(tmpText, "Created message should have TMP_Text component");
        Assert.AreEqual("TestUser : Test message", tmpText.text, "Message text should be formatted correctly");
    }

    [Test]
    public void CreateMessage_ShouldSetScrollToBottom()
    {
        string username = "TestUser";
        string message = "Test message";
        scrollRect.verticalNormalizedPosition = 1f;
        
        chatManager.CreateMessage(username, message);
        
        Assert.AreEqual(0f, scrollRect.verticalNormalizedPosition, "Should scroll to bottom");
    }

    [Test]
    public void CreateMessage_MultipleMessages_ShouldCreateMultipleObjects()
    {
        int initialCount = chatContent.childCount;
        
        chatManager.CreateMessage("User1", "Message1");
        chatManager.CreateMessage("User2", "Message2");
        chatManager.CreateMessage("User3", "Message3");
        
        Assert.AreEqual(initialCount + 3, chatContent.childCount, "Should create multiple messages");
    }

    [Test]
    public void CreateMessage_WithAudioSource_ShouldPlaySound()
    {
        string username = "TestUser";
        string message = "Test message";
        
        Assert.DoesNotThrow(() => chatManager.CreateMessage(username, message), 
            "Should not throw when playing sound");
    }

    [Test]
    public void CreateMessage_WithUserFound_ShouldShowPopup()
    {
        string username = "TestUser";
        string message = "Test message";
        
        var user = CreateMockUserWithPopup(username);
        UserManager.instance.Users.Add(user);
        
        chatManager.CreateMessage(username, message);
        
        Assert.Greater(chatContent.childCount, 0, "Should create message");
        LogAssert.Expect(LogType.Log, "Utente trovato: " + username);
    }

    [Test]
    public void CreateMessage_WithEmptyInputs_ShouldHandleGracefully()
    {
        Assert.DoesNotThrow(() => chatManager.CreateMessage("", ""), "Should handle empty inputs");
        Assert.DoesNotThrow(() => chatManager.CreateMessage(null, null), "Should handle null inputs");
    }
    
    [Test]
    public void Singleton_SecondInstance_ShouldDestroyItself()
    {
        var go1 = new GameObject("ChatManager1");
        var chatManager1 = go1.AddComponent<ChatManager>();
        chatManager1.Start();
        
        var go2 = new GameObject("ChatManager2");
        var chatManager2 = go2.AddComponent<ChatManager>();
        
        chatManager2.Start();
        
        Assert.AreEqual(chatManager, ChatManager.instance, "First instance should remain the singleton");
        
        Object.DestroyImmediate(go1);
        Object.DestroyImmediate(go2);
    }
    

    private User CreateMockUser(string rtmId)
    {
        return new User(rtmId, "rtc_" + rtmId, null, "testChannel");
    }

    private User CreateMockUserWithPopup(string rtmId)
    {
        var anchor = new GameObject("Anchor");
        var popup = anchor.AddComponent<Popup>();
        createdObjects.Add(anchor);
        
        return new User(rtmId, "rtc_" + rtmId, anchor, "testChannel");
    }

    private void SetPrivateField(string fieldName, object value)
    {
        var field = typeof(ChatManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(chatManager, value);
    }
}