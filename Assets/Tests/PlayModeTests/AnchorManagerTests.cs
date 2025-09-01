using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Moq;


[TestFixture]
public class AnchorManagerTests
{
    private AnchorManager anchorManager;
    private GameObject testGameObject;
    private UserManager userManager;

    [SetUp]
    public void SetUp()
    {
        userManager = new GameObject().AddComponent<UserManager>();
        UserManager.instance = userManager;
        // Crea un GameObject per i test
        testGameObject = new GameObject("TestAnchorManager");
        anchorManager = testGameObject.AddComponent<AnchorManager>();
        
        // Setup del singleton instance
        AnchorManager.instance = anchorManager;
        
        // Inizializza ancoraProva
        anchorManager.ancoraProva = new GameObject("TestAnchor");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate( userManager.gameObject);
        if (testGameObject != null)
            Object.DestroyImmediate(testGameObject);
        
        if (anchorManager.ancoraProva != null)
            Object.DestroyImmediate(anchorManager.ancoraProva);
            
        AnchorManager.instance = null;
    }

    [Test]
    public void Start_ShouldSetInstanceCorrectly()
    {
        // Arrange & Act
        anchorManager.Start();
        
        // Assert
        Assert.AreEqual(anchorManager, AnchorManager.instance);
    }

    [Test]
    public void Start_ShouldDestroyDuplicateInstance()
    {
        // Arrange
        var duplicateGameObject = new GameObject("DuplicateAnchorManager");
        var duplicateAnchorManager = duplicateGameObject.AddComponent<AnchorManager>();
        AnchorManager.instance = anchorManager; // Imposta l'istanza originale
        
        // Act
        duplicateAnchorManager.Start();
        
        // Assert
        Assert.AreEqual(anchorManager, AnchorManager.instance);
        // Il duplicato dovrebbe essere distrutto automaticamente
    }

    [Test]
    public void AddAnchor_ShouldAddAnchorToLists()
    {
        // Arrange
        var testAnchor = new GameObject("TestAnchor");
        
        // Act
        anchorManager.AddAnchor(testAnchor);
        
        // Assert
        Assert.Contains(testAnchor, anchorManager.anchors);
        Assert.Contains(testAnchor, anchorManager.FreeAnchors);
        
        // Cleanup
        Object.DestroyImmediate(testAnchor);
    }

    [Test]
    public void AssignAnchorToUser_ShouldAssignAncoraProvaToUser()
    {
        // Arrange
        var testUser = new User ( "12345", "TestChannel",  null,"test");
        
        // Act
        anchorManager.AssignAnchorToUser(testUser);
        
        // Assert
        Assert.AreEqual(anchorManager.ancoraProva, testUser.Anchor);
    }

    [Test]
    public void RemoveAnchor_ShouldRemoveAnchorFromLists()
    {
        // Arrange
        var testAnchor = new GameObject("TestAnchor");
        anchorManager.AddAnchor(testAnchor);
        
        // Act
        anchorManager.RemoveAnchor(testAnchor);
        
        // Assert
        Assert.IsFalse(anchorManager.anchors.Contains(testAnchor));
        Assert.IsFalse(anchorManager.FreeAnchors.Contains(testAnchor));
    }

    [Test]
    public void RemoveAnchor_WithAssignedUser_ShouldResetUserAnchor()
    {
        // Arrange
        var testAnchor = new GameObject("TestAnchor");
        var testUser = new User ( "12345", "TestChannel",  testAnchor,"test");
        
        // Mock UserManager per restituire l'utente
        userManager.Users = new List<User> { testUser };;
        // Aggiungi l'anchor
        anchorManager.AddAnchor(testAnchor);
        
        // Act
        anchorManager.RemoveAnchor(testAnchor);
        
        // Assert
        Assert.IsNull(testUser.Anchor);
    }

    [Test]
    public void OnCallQuit_ShouldClearAllAnchors()
    {
        // Arrange
        var anchor1 = new GameObject("Anchor1");
        var anchor2 = new GameObject("Anchor2");
        anchorManager.AddAnchor(anchor1);
        anchorManager.AddAnchor(anchor2);
        
        // Act
        anchorManager.OnCallQuit();
        
        // Assert
        Assert.AreEqual(0, anchorManager.anchors.Count);
        Assert.AreEqual(0, anchorManager.FreeAnchors.Count);
    }

    [UnityTest]
    public IEnumerator TryAssignAnchorToWaitingUser_ShouldAssignWhenBothAvailable()
    {
        // Arrange
        var testAnchor = new GameObject("TestAnchor");
        //var videoSurface = testAnchor.AddComponent<VideoSurface>();
        
        var testUser = new User ( "12345", "TestChannel",  testAnchor,"test");
        var waitingUsers = new List<User> { testUser };
        
        // Mock UserManager
        if (UserManager.instance == null)
        {
            var userManagerGO = new GameObject("UserManager");
            var userManager = userManagerGO.AddComponent<UserManager>();
            UserManager.instance = userManager;
        }
        
        // Simula utenti in attesa
        UserManager.instance.WaitingUsers = waitingUsers;
        
        // Act
        anchorManager.AddAnchor(testAnchor);
        
        yield return null; // Aspetta un frame
        
        // Assert
        Assert.AreEqual(testAnchor, testUser.Anchor);
        Assert.AreEqual(0, anchorManager.FreeAnchors.Count);
        
        // Cleanup
        Object.DestroyImmediate(testAnchor);
        if (UserManager.instance != null)
            Object.DestroyImmediate(UserManager.instance.gameObject);
    }
}
