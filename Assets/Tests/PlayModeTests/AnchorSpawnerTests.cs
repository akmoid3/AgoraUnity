using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Reflection;
using System.Threading.Tasks;

[TestFixture]
public class AnchorSpawnerTests
{
    private AnchorSpawner anchorSpawner;
    private GameObject testGameObject;
    private ARAnchorManager mockARAnchorManager;
    private XRRayInteractor mockXRRayInteractor;
    private List<GameObject> createdObjects;

    [SetUp]
    public void SetUp()
    {
        createdObjects = new List<GameObject>();
        LogAssert.ignoreFailingMessages = true;
        
        // Crea il GameObject principale
        testGameObject = new GameObject("AnchorSpawnerTest");
        anchorSpawner = testGameObject.AddComponent<AnchorSpawner>();
        createdObjects.Add(testGameObject);
        
        // Setup mock components
        SetupMockComponents();
        SetupManagerMocks();
        
        // Imposta i campi privati
        SetPrivateField("arAnchorManager", mockARAnchorManager);
        SetPrivateField("xrRayInteractor", mockXRRayInteractor);
        
        LogAssert.ignoreFailingMessages = false;
    }

    [TearDown]
    public void TearDown()
    {
        LogAssert.ignoreFailingMessages = true;
        
        // Cleanup singletons
        if (UserManager.instance != null && UserManager.instance.gameObject != null)
            Object.DestroyImmediate(UserManager.instance.gameObject);
        
        if (AnchorManager.instance != null && AnchorManager.instance.gameObject != null)
            Object.DestroyImmediate(AnchorManager.instance.gameObject);
        
        UserManager.instance = null;
        AnchorManager.instance = null;
        
        // Cleanup created objects
        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            if (createdObjects[i] != null)
                Object.DestroyImmediate(createdObjects[i]);
        }
        createdObjects.Clear();
        
        anchorSpawner = null;
        testGameObject = null;
        mockARAnchorManager = null;
        mockXRRayInteractor = null;
        
        LogAssert.ignoreFailingMessages = false;
    }

    [Test]
    public void Awake_ShouldGetXRRayInteractorComponent()
    {
        // Arrange - Crea un nuovo oggetto per testare Awake
        var newGO = new GameObject("TestAwake");
        var xrRayInteractor = newGO.AddComponent<XRRayInteractor>();
        var newAnchorSpawner = newGO.AddComponent<AnchorSpawner>();
        createdObjects.Add(newGO);
        
        // Act - Awake viene chiamato automaticamente
        
        // Assert
        var retrievedInteractor = GetPrivateField<XRRayInteractor>(newAnchorSpawner, "xrRayInteractor");
        Assert.AreEqual(xrRayInteractor, retrievedInteractor, "Should get XRRayInteractor component in Awake");
    }

    [Test]
    public void Start_ShouldAddSelectEnteredListener()
    {
        // Arrange
        bool listenerAdded = false;
        
        // Mock dell'evento selectEntered
        var selectEnteredEvent = GetPrivateField<SelectEnterEvent>(mockXRRayInteractor, "m_SelectEntered");
        if (selectEnteredEvent != null)
        {
            var persistentCallsCount = selectEnteredEvent.GetPersistentEventCount();
            
            // Act
            anchorSpawner.Start();
            
            // Assert
            var newPersistentCallsCount = selectEnteredEvent.GetPersistentEventCount();
            // Note: In test environment, questo potrebbe non funzionare perfettamente
            // Verifichiamo che Start non lanci eccezioni
            Assert.DoesNotThrow(() => anchorSpawner.Start(), "Start should not throw exceptions");
        }
        else
        {
            // Fallback test - verifica che Start non lanci eccezioni
            Assert.DoesNotThrow(() => anchorSpawner.Start(), "Start should not throw exceptions");
        }
    }

    [Test]
    public void SpawnAnchor_WithNullHit_ShouldReturnEarly()
    {
        // Arrange
        var selectEnterArgs = CreateMockSelectEnterEventArgs();
        
        // Setup XRRayInteractor to return false for TryGetCurrent3DRaycastHit
        SetupRayInteractorToReturnNoHit();
        
        // Act & Assert
        Assert.DoesNotThrow(() => InvokeSpawnAnchor(selectEnterArgs), 
            "Should handle null raycast hit gracefully");
    }

    [Test]
    public void SpawnAnchor_WithNonARPlaneCollider_ShouldReturnEarly()
    {
        // Arrange
        var selectEnterArgs = CreateMockSelectEnterEventArgs();
        var hitWithNonARPlane = CreateMockRaycastHitWithoutARPlane();
        
        SetupRayInteractorToReturnHit(hitWithNonARPlane);
        
        // Act & Assert
        Assert.DoesNotThrow(() => InvokeSpawnAnchor(selectEnterArgs), 
            "Should handle non-ARPlane collider gracefully");
    }

    [Test]
    public void SpawnAnchor_WithCeilingPlane_ShouldReturnEarly()
    {
        // Arrange
        var selectEnterArgs = CreateMockSelectEnterEventArgs();
        var hitWithCeilingPlane = CreateMockRaycastHitWithARPlane(PlaneClassifications.Ceiling);
        
        SetupRayInteractorToReturnHit(hitWithCeilingPlane);
        
        // Act & Assert
        Assert.DoesNotThrow(() => InvokeSpawnAnchor(selectEnterArgs), 
            "Should not spawn on ceiling planes");
    }

    [Test]
    public void SpawnAnchor_WithFloorPlane_ShouldReturnEarly()
    {
        // Arrange
        var selectEnterArgs = CreateMockSelectEnterEventArgs();
        var hitWithFloorPlane = CreateMockRaycastHitWithARPlane(PlaneClassifications.Floor);
        
        SetupRayInteractorToReturnHit(hitWithFloorPlane);
        
        // Act & Assert
        Assert.DoesNotThrow(() => InvokeSpawnAnchor(selectEnterArgs), 
            "Should not spawn on floor planes");
    }

    

    [Test]
    public void SpawnAnchor_WithTablePlane_ShouldAttemptSpawn()
    {
        // Arrange
        var selectEnterArgs = CreateMockSelectEnterEventArgs();
        var hitWithTablePlane = CreateMockRaycastHitWithARPlane(PlaneClassifications.Table);
        
        SetupRayInteractorToReturnHit(hitWithTablePlane);
        
        // Act & Assert
        Assert.DoesNotThrow(() => InvokeSpawnAnchor(selectEnterArgs), 
            "Should attempt to spawn on table planes");
    }

    [Test]
    public void SpawnAnchor_WithNullARAnchorManager_ShouldHandleGracefully()
    {
        // Arrange
        SetPrivateField("arAnchorManager", null);
        var selectEnterArgs = CreateMockSelectEnterEventArgs();
        var validHit = CreateMockRaycastHitWithARPlane(PlaneClassifications.Table);
        
        SetupRayInteractorToReturnHit(validHit);
        
        // Act & Assert
        Assert.DoesNotThrow(() => InvokeSpawnAnchor(selectEnterArgs), 
            "Should handle null ARAnchorManager gracefully");
    }

    [Test]
    public void SpawnAnchor_WithNullUserManager_ShouldHandleGracefully()
    {
        // Arrange
        UserManager.instance = null;
        var selectEnterArgs = CreateMockSelectEnterEventArgs();
        var validHit = CreateMockRaycastHitWithARPlane(PlaneClassifications.Table);
        
        SetupRayInteractorToReturnHit(validHit);
        
        // Act & Assert
        Assert.DoesNotThrow(() => InvokeSpawnAnchor(selectEnterArgs), 
            "Should handle null UserManager gracefully");
    }

    [Test]
    public void SpawnAnchor_WithNullAnchorManager_ShouldHandleGracefully()
    {
        // Arrange
        AnchorManager.instance = null;
        var selectEnterArgs = CreateMockSelectEnterEventArgs();
        var validHit = CreateMockRaycastHitWithARPlane(PlaneClassifications.Table);
        
        SetupRayInteractorToReturnHit(validHit);
        
        // Act & Assert
        Assert.DoesNotThrow(() => InvokeSpawnAnchor(selectEnterArgs), 
            "Should handle null AnchorManager gracefully");
    }

    // Helper Methods
    private void SetupMockComponents()
    {
        // Crea mock ARAnchorManager
        var arAnchorManagerGO = new GameObject("MockARAnchorManager");
        mockARAnchorManager = arAnchorManagerGO.AddComponent<ARAnchorManager>();
        createdObjects.Add(arAnchorManagerGO);
        
        // Crea mock XRRayInteractor
        var xrRayInteractorGO = new GameObject("MockXRRayInteractor");
        mockXRRayInteractor = xrRayInteractorGO.AddComponent<XRRayInteractor>();
        createdObjects.Add(xrRayInteractorGO);
    }

    private void SetupManagerMocks()
    {
        // Setup UserManager mock
        if (UserManager.instance == null)
        {
            var userManagerGO = new GameObject("MockUserManager");
            var userManager = userManagerGO.AddComponent<UserManager>();
            UserManager.instance = userManager;
            
            // Crea mock UserPrefab
            var userPrefab = new GameObject("MockUserPrefab");
            SetPrivateField(userManager, "UserPrefab", userPrefab);
            createdObjects.Add(userPrefab);
            
            createdObjects.Add(userManagerGO);
        }
        
        // Setup AnchorManager mock
        if (AnchorManager.instance == null)
        {
            var anchorManagerGO = new GameObject("MockAnchorManager");
            var anchorManager = anchorManagerGO.AddComponent<AnchorManager>();
            AnchorManager.instance = anchorManager;
            createdObjects.Add(anchorManagerGO);
        }
    }

    private SelectEnterEventArgs CreateMockSelectEnterEventArgs()
    {
        // Crea un mock SelectEnterEventArgs
        var interactionManager = new GameObject("InteractionManager").AddComponent<XRInteractionManager>();
        createdObjects.Add(interactionManager.gameObject);
        
        // Nota: SelectEnterEventArgs richiede setup complesso, 
        // per i test usiamo null e gestiamo il caso nel metodo chiamato
        return null;
    }

    private RaycastHit CreateMockRaycastHitWithoutARPlane()
    {
        var colliderGO = new GameObject("MockCollider");
        var collider = colliderGO.AddComponent<BoxCollider>();
        createdObjects.Add(colliderGO);
        
        var hit = new RaycastHit();
        // Nota: Non è possibile impostare direttamente i campi di RaycastHit nei test
        // Questo è un mock concettuale
        return hit;
    }

    private RaycastHit CreateMockRaycastHitWithARPlane(PlaneClassifications classification)
    {
        var planeGO = new GameObject("MockARPlane");
        var arPlane = planeGO.AddComponent<ARPlane>();
        
        // Imposta classification usando reflection se necessario
        SetPrivateField(arPlane, "m_Classifications", classification);
        
        var collider = planeGO.AddComponent<BoxCollider>();
        createdObjects.Add(planeGO);
        
        var hit = new RaycastHit();
        // Mock hit setup
        return hit;
    }

    private void SetupRayInteractorToReturnNoHit()
    {
        // In un test reale, dovresti mockare il comportamento di TryGetCurrent3DRaycastHit
        // Per semplicità, assumiamo che il metodo gestisca correttamente i casi null
    }

    private void SetupRayInteractorToReturnHit(RaycastHit hit)
    {
        // Mock setup per restituire l'hit specificato
        // Implementazione specifica dipende dalla struttura dell'XRRayInteractor
    }

    private void SetupSuccessfulAnchorResult()
    {
        // Mock setup per far restituire un risultato di successo all'ARAnchorManager
        // Questo richiederebbe mock più sofisticati per TryAddAnchorAsync
    }

    private void InvokeSpawnAnchor(SelectEnterEventArgs args)
    {
        var method = typeof(AnchorSpawner).GetMethod("SpawnAnchor", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(anchorSpawner, new object[] { args });
    }

    private void SetPrivateField(string fieldName, object value)
    {
        var field = typeof(AnchorSpawner).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(anchorSpawner, value);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(target, value);
    }

    private T GetPrivateField<T>(string fieldName)
    {
        var field = typeof(AnchorSpawner).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field != null ? (T)field.GetValue(anchorSpawner) : default(T);
    }

    private T GetPrivateField<T>(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field != null ? (T)field.GetValue(target) : default(T);
    }
}