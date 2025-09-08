using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;

[TestFixture]
public class NameDisplayerTests
{
    private NameDisplayer nameDisplayer;
    private GameObject testGameObject;
    private Transform displayParent;
    private List<GameObject> mockLetterPrefabs;

    [SetUp]
    public void SetUp()
    {
        // Supprime i log di errore per i test
        LogAssert.ignoreFailingMessages = true;
        
        // Crea il display parent
        var displayParentGO = new GameObject("DisplayParent");
        displayParent = displayParentGO.transform;
        displayParent.position = Vector3.zero;
        displayParent.rotation = Quaternion.identity;
        
        // Crea mock letter prefabs per A-Z
        CreateMockLetterPrefabs();
        
        // Crea il GameObject e aggiunge il componente
        // (Awake verrà chiamato automaticamente ma fallirà silenziosamente)
        testGameObject = new GameObject("NameDisplayerTest");
        nameDisplayer = testGameObject.AddComponent<NameDisplayer>();
        
        // Ora imposta i campi correttamente DOPO che il componente è stato aggiunto
        SetPrivateField("displayParent", displayParent);
        SetPrivateField("letterPrefabs", mockLetterPrefabs);
        SetPrivateField("letterSpacing", 0.1f);
        
        // Inizializza manualmente la letterMap chiamando di nuovo Awake
        // ma questa volta con tutti i campi impostati correttamente
        InitializeLetterMap();

        nameDisplayer.Awake();
        // Ripristina la gestione normale dei log
        LogAssert.ignoreFailingMessages = false;
    }

    [TearDown]
    public void TearDown()
    {
        // Pulisci tutti i GameObject creati durante i test
        if (testGameObject != null)
            Object.DestroyImmediate(testGameObject);
        
        if (displayParent != null)
            Object.DestroyImmediate(displayParent.gameObject);
        
        // Pulisci i mock prefabs
        if (mockLetterPrefabs != null)
        {
            foreach (var prefab in mockLetterPrefabs)
            {
                if (prefab != null)
                    Object.DestroyImmediate(prefab);
            }
        }
        
        LogAssert.ignoreFailingMessages = false;
    }

    [Test]
    public void DisplayName_WithValidName_ShouldInstantiateCorrectLetters()
    {
        // Arrange
        string testName = "ABC";
        
        // Act
        nameDisplayer.DisplayName(testName);
        
        // Assert
        var instantiatedLetters = GetInstantiatedLetters();
        Assert.AreEqual(3, instantiatedLetters.Count, "Should instantiate 3 letters for 'ABC'");
        
        // Verifica che le lettere siano figlie del display parent
        foreach (var letter in instantiatedLetters)
        {
            Assert.AreEqual(displayParent, letter.transform.parent);
        }
    }

    [Test]
    public void DisplayName_WithLowercaseName_ShouldConvertToUppercase()
    {
        // Arrange
        string testName = "abc";
        
        // Act
        nameDisplayer.DisplayName(testName);
        
        // Assert
        var instantiatedLetters = GetInstantiatedLetters();
        Assert.AreEqual(3, instantiatedLetters.Count, "Should handle lowercase letters by converting to uppercase");
    }

    [Test]
    public void DisplayName_WithSpaces_ShouldHandleSpacesCorrectly()
    {
        // Arrange
        string testName = "A B";
        
        // Act
        nameDisplayer.DisplayName(testName);
        
        // Assert
        var instantiatedLetters = GetInstantiatedLetters();
        Assert.AreEqual(2, instantiatedLetters.Count, "Should instantiate 2 letters for 'A B' (space doesn't create object)");
    }

    [Test]
    public void DisplayName_WithEmptyString_ShouldNotInstantiateAnyLetters()
    {
        // Act
        nameDisplayer.DisplayName("");
        
        // Assert
        var instantiatedLetters = GetInstantiatedLetters();
        Assert.AreEqual(0, instantiatedLetters.Count, "Should not instantiate any letters for empty string");
    }

    [Test]
    public void DisplayName_WithNullString_ShouldNotInstantiateAnyLetters()
    {
        // Act
        nameDisplayer.DisplayName(null);
        
        // Assert
        var instantiatedLetters = GetInstantiatedLetters();
        Assert.AreEqual(0, instantiatedLetters.Count, "Should not instantiate any letters for null string");
    }

    [Test]
    public void DisplayName_CalledMultipleTimes_ShouldClearPreviousLetters()
    {
        // Arrange & Act
        nameDisplayer.DisplayName("ABC");
        var firstCallCount = GetInstantiatedLetters().Count;
        
        nameDisplayer.DisplayName("XY");
        var secondCallCount = GetInstantiatedLetters().Count;
        
        // Assert
        Assert.AreEqual(3, firstCallCount, "First call should create 3 letters");
        Assert.AreEqual(2, secondCallCount, "Second call should clear previous and create 2 new letters");
        
    }

    [Test]
    public void DisplayName_WithInvalidCharacters_ShouldSkipInvalidCharacters()
    {
        // Arrange
        string testName = "A1B2C"; // Include numeri che non dovrebbero avere prefab
        
        // Act
        nameDisplayer.DisplayName(testName);
        
        // Assert
        var instantiatedLetters = GetInstantiatedLetters();
        Assert.AreEqual(3, instantiatedLetters.Count, "Should only instantiate letters A, B, C and skip numbers");
    }

    [Test]
    public void DisplayName_WithNoValidCharacters_ShouldHaveZeroInstantiatedPrefabs()
    {
        // Arrange
        string testName = "123!@#"; // Solo caratteri non-alfabetici
    
        // Act
        nameDisplayer.DisplayName(testName);
    
        // Assert
        var instantiatedLetters = GetInstantiatedLetters();
        Assert.AreEqual(0, instantiatedLetters.Count, "Should instantiate 0 letters for non-alphabetic characters");
        Assert.AreEqual(0, displayParent.childCount, "Display parent should have no children");
    }
    
    [Test]
    public void DisplayName_ShouldPositionLettersWithCorrectSpacing()
    {
        // Arrange
        string testName = "AB";
        
        // Act
        nameDisplayer.DisplayName(testName);
        
        // Assert
        var instantiatedLetters = GetInstantiatedLetters();
        Assert.AreEqual(2, instantiatedLetters.Count);
        
        // Verifica che le lettere siano posizionate diversamente
        if (instantiatedLetters.Count >= 2)
        {
            Vector3 firstLetterPos = instantiatedLetters[0].transform.position;
            Vector3 secondLetterPos = instantiatedLetters[1].transform.position;
            
            Assert.AreNotEqual(firstLetterPos, secondLetterPos, "Letters should be positioned differently");
        }
    }

    [Test]
    public void DisplayName_ShouldCenterLettersAfterPositioning()
    {
        // Arrange
        string testName = "ABC";
        Vector3 originalParentPosition = displayParent.position;
        
        // Act
        nameDisplayer.DisplayName(testName);
        
        // Assert
        var instantiatedLetters = GetInstantiatedLetters();
        
        if (instantiatedLetters.Count > 0)
        {
            // Calcola la posizione media delle lettere
            Vector3 totalPosition = Vector3.zero;
            foreach (var letter in instantiatedLetters)
            {
                totalPosition += letter.transform.position;
            }
            Vector3 averagePosition = totalPosition / instantiatedLetters.Count;
            
            // La posizione media dovrebbe essere vicina alla posizione del parent
            float distance = Vector3.Distance(averagePosition, originalParentPosition);
            Assert.Less(distance, 2.0f, "Letters should be centered around parent position");
        }
    }

    [Test]
    public void Awake_ShouldInitializeLetterMapCorrectly()
    {
        // Assert - Verifica usando reflection che la letterMap sia stata inizializzata
        var letterMap = GetPrivateField<Dictionary<char, GameObject>>("letterMap");
        
        Assert.IsNotNull(letterMap, "Letter map should be initialized");
        Assert.AreEqual(26, letterMap.Count, "Letter map should contain 26 entries (A-Z)");
        
        // Verifica che alcune lettere specifiche siano mappate correttamente
        Assert.IsTrue(letterMap.ContainsKey('A'), "Letter map should contain 'A'");
        Assert.IsTrue(letterMap.ContainsKey('Z'), "Letter map should contain 'Z'");
    }

    [Test]
    public void LetterMap_ShouldMapCorrectPrefabsToLetters()
    {
        // Arrange & Assert
        var letterMap = GetPrivateField<Dictionary<char, GameObject>>("letterMap");
        
        // Verifica che ogni lettera sia mappata al prefab corretto
        for (int i = 0; i < 26 && i < mockLetterPrefabs.Count; i++)
        {
            char expectedLetter = (char)('A' + i);
            Assert.IsTrue(letterMap.ContainsKey(expectedLetter), $"Should contain letter {expectedLetter}");
            Assert.AreEqual(mockLetterPrefabs[i], letterMap[expectedLetter], $"Letter {expectedLetter} should map to correct prefab");
        }
    }

    // Helper methods per la gestione manuale dell'inizializzazione
    private void InitializeLetterMap()
    {
        // Ricrea manualmente la logica di Awake() per inizializzare la letterMap
        var letterMap = new Dictionary<char, GameObject>();
        var letterPrefabs = GetPrivateField<List<GameObject>>("letterPrefabs");
        
        if (letterPrefabs != null)
        {
            for (int i = 0; i < letterPrefabs.Count && i < 26; i++)
            {
                char letter = (char)('A' + i);
                if (letterPrefabs[i] != null)
                {
                    letterMap[letter] = letterPrefabs[i];
                }
            }
        }
        
        SetPrivateField("letterMap", letterMap);
    }

    // Helper methods per reflection
    private void SetPrivateField(string fieldName, object value)
    {
        var field = typeof(NameDisplayer).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(nameDisplayer, value);
    }

    private T GetPrivateField<T>(string fieldName)
    {
        var field = typeof(NameDisplayer).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field != null ? (T)field.GetValue(nameDisplayer) : default(T);
    }

    private void CreateMockLetterPrefabs()
    {
        mockLetterPrefabs = new List<GameObject>();
        
        for (int i = 0; i < 26; i++)
        {
            char letter = (char)('A' + i);
            mockLetterPrefabs.Add(CreateSingleMockPrefab(letter));
        }
    }

    private GameObject CreateSingleMockPrefab(char letter)
    {
        GameObject mockPrefab = new GameObject($"Letter_{letter}");
        
        // Aggiungi un MeshRenderer per simulare un prefab visuale
        var meshRenderer = mockPrefab.AddComponent<MeshRenderer>();
        var meshFilter = mockPrefab.AddComponent<MeshFilter>();
        
        // Crea bounds fittizi per il renderer
        var material = new Material(Shader.Find("Standard"));
        meshRenderer.material = material;
        
        // Simula dimensioni diverse per ogni lettera
        float width = 0.5f + ((letter - 'A') * 0.01f);
        SetMockRendererBounds(meshRenderer, width);
        
        return mockPrefab;
    }

    private void SetMockRendererBounds(MeshRenderer renderer, float width)
    {
        // Crea una mesh semplice per dare bounds al renderer
        var mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-width/2, -0.5f, 0),
            new Vector3(width/2, -0.5f, 0),
            new Vector3(width/2, 0.5f, 0),
            new Vector3(-width/2, 0.5f, 0)
        };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateBounds();
        
        renderer.GetComponent<MeshFilter>().mesh = mesh;
    }

    private List<GameObject> GetInstantiatedLetters()
    {
        return GetPrivateField<List<GameObject>>("instantiatedLetters") ?? new List<GameObject>();
    }
}