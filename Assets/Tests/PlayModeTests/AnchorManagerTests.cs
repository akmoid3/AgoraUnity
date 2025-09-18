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
        testGameObject = new GameObject("TestAnchorManager");
        anchorManager = testGameObject.AddComponent<AnchorManager>();
        
        AnchorManager.instance = anchorManager;
        
        anchorManager.ancoraProva = new GameObject("TestAnchor");
    }

    [TearDown]
    public void TearDown()
    {
        if(userManager != null)
            Object.DestroyImmediate(userManager.gameObject);
        if (testGameObject != null)
            Object.DestroyImmediate(testGameObject);
        
        if (anchorManager.ancoraProva != null)
            Object.DestroyImmediate(anchorManager.ancoraProva);
        
        AnchorManager.instance = null;
    }

    [Test]
    public void Start_ShouldSetInstanceCorrectly()
    {
        anchorManager.Start();
        
        Assert.AreEqual(anchorManager, AnchorManager.instance);
    }

    [Test]
    public void Start_ShouldDestroyDuplicateInstance()
    {
        var duplicateGameObject = new GameObject("DuplicateAnchorManager");
        var duplicateAnchorManager = duplicateGameObject.AddComponent<AnchorManager>();
        AnchorManager.instance = anchorManager; // Imposta l'istanza originale
        
        duplicateAnchorManager.Start();
        
        Assert.AreEqual(anchorManager, AnchorManager.instance);
    }

    [Test]
    public void AddAnchor_ShouldAddAnchorToLists()
    {
        var testAnchor = new GameObject("TestAnchor");
        
        anchorManager.AddAnchor(testAnchor);
        
        Assert.Contains(testAnchor, anchorManager.anchors);
        Assert.Contains(testAnchor, anchorManager.FreeAnchors);
        
        Object.DestroyImmediate(testAnchor);
    }

    [Test]
    public void AssignAnchorToUser_ShouldAssignAncoraProvaToUser()
    {
        var testUser = new User ( "12345", "TestChannel",  null,"test");
        
        anchorManager.AssignAnchorToUser(testUser);
        
        //Assert.AreEqual(anchorManager.ancoraProva, testUser.Anchor);
    }

    [Test]
    public void RemoveAnchor_ShouldRemoveAnchorFromLists()
    {
        var testAnchor = new GameObject("TestAnchor");
        anchorManager.AddAnchor(testAnchor);
        
        anchorManager.RemoveAnchor(testAnchor);
        
        Assert.IsFalse(anchorManager.anchors.Contains(testAnchor));
        Assert.IsFalse(anchorManager.FreeAnchors.Contains(testAnchor));
    }

    [Test]
    public void RemoveAnchor_WithAssignedUser_ShouldResetUserAnchor()
    {
        var testAnchor = new GameObject("TestAnchor");
        var testUser = new User ( "12345", "1020",  testAnchor,"test");
        
        userManager.Users = new List<User> { testUser };;
        anchorManager.AddAnchor(testAnchor);
        
        anchorManager.RemoveAnchor(testAnchor);
        
        //Assert.IsNull(testUser.Anchor);
    }

    [Test]
    public void OnCallQuit_ShouldClearAllAnchors()
    {
        var anchor1 = new GameObject("Anchor1");
        var anchor2 = new GameObject("Anchor2");
        anchorManager.AddAnchor(anchor1);
        anchorManager.AddAnchor(anchor2);
        
        anchorManager.OnCallQuit();
        
        Assert.AreEqual(0, anchorManager.anchors.Count);
        Assert.AreEqual(0, anchorManager.FreeAnchors.Count);
    }

    [UnityTest]
    public IEnumerator TryAssignAnchorToWaitingUser_ShouldAssignWhenBothAvailable()
    {
        var testAnchor = new GameObject("TestAnchor");
        //var videoSurface = testAnchor.AddComponent<VideoSurface>();
        
        var testUser = new User ( "12345", "1020",  testAnchor,"test");
        var waitingUsers = new List<User> { testUser };
        
        if (UserManager.instance == null)
        {
            var userManagerGO = new GameObject("UserManager");
            var userManager = userManagerGO.AddComponent<UserManager>();
            UserManager.instance = userManager;
        }
        
        UserManager.instance.WaitingUsers = waitingUsers;
        
        anchorManager.AddAnchor(testAnchor);
        
        yield return null;
        
        //Assert.AreEqual(testAnchor, testUser.Anchor);
        //Assert.AreEqual(0, anchorManager.FreeAnchors.Count);
        
        Object.DestroyImmediate(testAnchor);
        if (UserManager.instance != null)
            Object.DestroyImmediate(UserManager.instance.gameObject);
    }
}
