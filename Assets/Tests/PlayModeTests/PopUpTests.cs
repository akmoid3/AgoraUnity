using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using System.Reflection;

[TestFixture]
public class PopupTests
{
    private Popup popup;
    private GameObject testGameObject;
    private GameObject mockTextPrefab;
    private RectTransform textParent;
    private Canvas testCanvas;

    [SetUp]
    public void SetUp()
    {
        // Crea un Canvas per i test UI
        var canvasGO = new GameObject("TestCanvas");
        testCanvas = canvasGO.AddComponent<Canvas>();
        testCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Crea il GameObject principale per il test
        testGameObject = new GameObject("PopupTest");
        popup = testGameObject.AddComponent<Popup>();
        
        // Crea il text parent
        var textParentGO = new GameObject("TextParent");
        textParent = textParentGO.AddComponent<RectTransform>();
        textParent.SetParent(testCanvas.transform);
        textParent.sizeDelta = new Vector2(200, 100);
        
        // Crea un parent per textParent (necessario per RecenterParent)
        var parentGO = new GameObject("Parent");
        var parentRect = parentGO.AddComponent<RectTransform>();
        parentRect.SetParent(testCanvas.transform);
        parentRect.sizeDelta = new Vector2(300, 200);
        textParent.SetParent(parentRect);
        
        // Crea mock text prefab
        CreateMockTextPrefab();
        
        // Imposta i campi usando reflection
        SetPrivateField("textPrefab", mockTextPrefab);
        SetPrivateField("textParent", textParent);
        SetPrivateField("maxCharsPerLine", 18);
        SetPrivateField("maxLines", 3);
    }

    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
            Object.DestroyImmediate(testGameObject);
        
        if (mockTextPrefab != null)
            Object.DestroyImmediate(mockTextPrefab);
        
        if (testCanvas != null)
            Object.DestroyImmediate(testCanvas.gameObject);
    }

    [Test]
    public void ShowMessage_WithValidMessage_ShouldCreateTextObject()
    {
        // Arrange
        string testMessage = "Test message";
        int initialChildCount = textParent.childCount;
        
        // Act
        popup.ShowMessage(testMessage, 10f); // Lunga durata per il test
        
        // Assert
        Assert.AreEqual(initialChildCount + 1, textParent.childCount, "Should create one text object");
        
        var createdChild = textParent.GetChild(textParent.childCount - 1);
        var tmpText = createdChild.GetComponent<TMP_Text>();
        Assert.IsNotNull(tmpText, "Created object should have TMP_Text component");
        Assert.AreEqual(testMessage, tmpText.text, "Text should match the input message");
    }

    [Test]
    public void ShowMessage_WithLongMessage_ShouldInsertLineBreaks()
    {
        // Arrange
        string longMessage = "This is a very long message that should be broken into multiple lines";
        
        // Act
        popup.ShowMessage(longMessage, 10f);
        
        // Assert
        var createdChild = textParent.GetChild(textParent.childCount - 1);
        var tmpText = createdChild.GetComponent<TMP_Text>();
        
        Assert.IsTrue(tmpText.text.Contains("\n"), "Long message should contain line breaks");
    }

    [Test]
    public void ShowMessage_WithVeryLongMessage_ShouldTruncateAfterMaxLines()
    {
        // Arrange
        // Crea un messaggio che supera maxLines (3 righe)
        string veryLongMessage = new string('A', 18 * 5); // 5 righe di caratteri
        
        // Act
        popup.ShowMessage(veryLongMessage, 10f);
        
        // Assert
        var createdChild = textParent.GetChild(textParent.childCount - 1);
        var tmpText = createdChild.GetComponent<TMP_Text>();
        
        string[] lines = tmpText.text.Split('\n');
        Assert.LessOrEqual(lines.Length, 4, "Should not exceed maxLines + ellipsis"); // 3 righe + "..."
        Assert.IsTrue(tmpText.text.EndsWith("..."), "Should end with ellipsis when truncated");
    }

    [Test]
    public void ShowMessage_WithEmptyString_ShouldHandleGracefully()
    {
        // Arrange
        string emptyMessage = "";
        
        // Act & Assert
        Assert.DoesNotThrow(() => popup.ShowMessage(emptyMessage, 1f), "Should handle empty string gracefully");
        
        var createdChild = textParent.GetChild(textParent.childCount - 1);
        var tmpText = createdChild.GetComponent<TMP_Text>();
        Assert.AreEqual("", tmpText.text, "Empty message should result in empty text");
    }



   

    [Test]
    public void ShowMessage_WithPrefabWithoutTMPText_ShouldLogWarningAndReturn()
    {
        // Arrange
        var invalidPrefab = new GameObject("InvalidPrefab");
        // Non aggiungere TMP_Text component
        SetPrivateField("textPrefab", invalidPrefab);
        
        LogAssert.Expect(LogType.Warning, "TMP_Text NULLLLLLLLLL!!!!");
        
        // Act
        popup.ShowMessage("test message", 1f);
        
        // Assert - Il warning dovrebbe essere stato loggato
        
        // Cleanup
        Object.DestroyImmediate(invalidPrefab);
    }

    [UnityTest]
    public IEnumerator ShowMessage_ShouldDestroyObjectAfterDuration()
    {
        // Arrange
        float testDuration = 0.1f; // Breve durata per il test
        int initialChildCount = textParent.childCount;
        
        // Act
        popup.ShowMessage("Test message", testDuration);
        
        // Assert - Verifica che l'oggetto sia stato creato
        Assert.AreEqual(initialChildCount + 1, textParent.childCount);
        
        // Wait for destruction
        yield return new WaitForSeconds(testDuration + 1f);
        
        // Assert - Verifica che l'oggetto sia stato distrutto
        Assert.AreEqual(initialChildCount, textParent.childCount, "Text object should be destroyed after duration");
    }

    [Test]
    public void ShowMessage_MultipleMessages_ShouldCreateMultipleObjects()
    {
        // Arrange
        int initialChildCount = textParent.childCount;
        
        // Act
        popup.ShowMessage("Message 1", 10f);
        popup.ShowMessage("Message 2", 10f);
        popup.ShowMessage("Message 3", 10f);
        
        // Assert
        Assert.AreEqual(initialChildCount + 3, textParent.childCount, "Should create multiple text objects");
    }

    [Test]
    public void InsertLineBreaks_WithNormalString_ShouldInsertBreaksCorrectly()
    {
        // Arrange
        string input = "This is a test message";
        int maxChars = 6;
        
        // Act
        string result = InvokePrivateMethod<string>("InsertLineBreaks", input, maxChars);
        
        // Assert
        string[] lines = result.Split('\n');
        foreach (string line in lines)
        {
            if (line.Length > 0) // Ignora righe vuote
            {
                Assert.LessOrEqual(line.Length, maxChars, $"Line '{line}' should not exceed {maxChars} characters");
            }
        }
    }

    [Test]
    public void InsertLineBreaks_WithEmptyString_ShouldReturnEmpty()
    {
        // Arrange
        string input = "";
        int maxChars = 10;
        
        // Act
        string result = InvokePrivateMethod<string>("InsertLineBreaks", input, maxChars);
        
        // Assert
        Assert.AreEqual("", result, "Empty string should return empty");
    }

    [Test]
    public void InsertLineBreaks_WithNullString_ShouldReturnNull()
    {
        // Arrange
        string input = null;
        int maxChars = 10;
        
        // Act
        string result = InvokePrivateMethod<string>("InsertLineBreaks", input, maxChars);
        
        // Assert
        Assert.IsNull(result, "Null string should return null");
    }

    [Test]
    public void InsertLineBreaks_WithExactLength_ShouldAddLineBreakAtEnd()
    {
        // Arrange
        string input = "123456"; // Esattamente 6 caratteri
        int maxChars = 6;
        
        // Act
        string result = InvokePrivateMethod<string>("InsertLineBreaks", input, maxChars);
        
        // Assert
        Assert.IsTrue(result.EndsWith("\n"), "Should add line break at exact length");
    }

    [Test]
    public void RecenterParent_WithNullTextParent_ShouldNotThrow()
    {
        // Arrange
        SetPrivateField("textParent", null);
        
        // Act & Assert
        Assert.DoesNotThrow(() => InvokePrivateMethod("RecenterParent"), "Should handle null textParent gracefully");
    }

    [Test]
    public void RecenterParent_WithSmallContent_ShouldCenterPivot()
    {
        // Arrange
        var parentRect = textParent.parent as RectTransform;
        parentRect.sizeDelta = new Vector2(300, 300); // Grande parent
        textParent.sizeDelta = new Vector2(100, 100); // Piccolo content
        
        // Act
        InvokePrivateMethod("RecenterParent");
        
        // Assert
        Assert.AreEqual(new Vector2(0.5f, 0.5f), textParent.pivot, "Should center pivot for small content");
        Assert.AreEqual(new Vector2(0.0f, -40.0f), textParent.anchoredPosition, "Should set correct anchored position");
    }

    [Test]
    public void RecenterParent_WithLargeContent_ShouldTopPivot()
    {
        // Arrange
        var parentRect = textParent.parent as RectTransform;
        parentRect.sizeDelta = new Vector2(200, 200); // Parent
        textParent.sizeDelta = new Vector2(300, 300); // Content più grande
        
        // Act
        InvokePrivateMethod("RecenterParent");
        
        // Assert
        Assert.AreEqual(new Vector2(0.5f, 1f), textParent.pivot, "Should set top pivot for large content");
        Assert.AreEqual(new Vector2(0.0f, -40.0f), textParent.anchoredPosition, "Should set correct anchored position");
    }

    // Helper methods
    private void CreateMockTextPrefab()
    {
        mockTextPrefab = new GameObject("MockTextPrefab");
        var tmpText = mockTextPrefab.AddComponent<TextMeshProUGUI>();
        tmpText.text = "";
        tmpText.fontSize = 14;
    }

    private void SetPrivateField(string fieldName, object value)
    {
        var field = typeof(Popup).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(popup, value);
    }

    private T GetPrivateField<T>(string fieldName)
    {
        var field = typeof(Popup).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field != null ? (T)field.GetValue(popup) : default(T);
    }

    private T InvokePrivateMethod<T>(string methodName, params object[] parameters)
    {
        var method = typeof(Popup).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)method?.Invoke(popup, parameters);
    }

    private void InvokePrivateMethod(string methodName, params object[] parameters)
    {
        var method = typeof(Popup).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(popup, parameters);
    }
}