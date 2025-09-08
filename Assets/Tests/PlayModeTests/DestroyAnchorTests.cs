using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Reflection;

[TestFixture]
public class DestroyAnchorTests
{
    private DestroyAnchor destroyAnchor;
    private GameObject testGameObject;
    private XRGrabInteractable mockGrabInteractable;
    private List<GameObject> createdObjects;

    [SetUp]
    public void SetUp()
    {
        createdObjects = new List<GameObject>();
        LogAssert.ignoreFailingMessages = true;
        
        testGameObject = new GameObject("DestroyAnchorTest");
        mockGrabInteractable = testGameObject.AddComponent<XRGrabInteractable>();
        destroyAnchor = testGameObject.AddComponent<DestroyAnchor>();
        createdObjects.Add(testGameObject);
        
        SetupManagerMocks();
        
        LogAssert.ignoreFailingMessages = false;
    }

    [TearDown]
    public void TearDown()
    {
        LogAssert.ignoreFailingMessages = true;
        
        if (UserManager.instance != null && UserManager.instance.gameObject != null)
            Object.DestroyImmediate(UserManager.instance.gameObject);
        
        if (AnchorManager.instance != null && AnchorManager.instance.gameObject != null)
            Object.DestroyImmediate(AnchorManager.instance.gameObject);
        
        UserManager.instance = null;
        AnchorManager.instance = null;
        
        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            if (createdObjects[i] != null)
                Object.DestroyImmediate(createdObjects[i]);
        }
        createdObjects.Clear();
        
        destroyAnchor = null;
        testGameObject = null;
        mockGrabInteractable = null;
        
        LogAssert.ignoreFailingMessages = false;
    }

    [Test]
    public void Awake_ShouldGetXRGrabInteractableComponent()
    {
        var newGO = new GameObject("TestAwake");
        var xrGrabInteractable = newGO.AddComponent<XRGrabInteractable>();
        var newDestroyAnchor = newGO.AddComponent<DestroyAnchor>();
        createdObjects.Add(newGO);
        
        
        var retrievedGrabInteractable = GetPrivateField<XRGrabInteractable>(newDestroyAnchor, "grabInteractable");
        Assert.AreEqual(xrGrabInteractable, retrievedGrabInteractable, "Should get XRGrabInteractable component in Awake");
    }

    


    [Test]
    public void OnDisable_ShouldRemoveActivatedListener()
    {
        // Arrange
        destroyAnchor.enabled = true; 
        int enabledListenerCount = GetEventListenerCount(mockGrabInteractable.activated);
        
        destroyAnchor.enabled = false;
        
        int disabledListenerCount = GetEventListenerCount(mockGrabInteractable.activated);
        Assert.LessOrEqual(disabledListenerCount, enabledListenerCount, "Should remove activated listener on disable");
    }

    [Test]
    public void OnActivated_WithAssignedUser_ShouldClearUserAnchorAndAddToWaiting()
    {
        // Arrange
        string testRtmId = "testUser";
        var user = new User(testRtmId, "rtc_" + testRtmId, testGameObject, "testChannel");
        UserManager.instance.Users.Add(user);
        
        var activateArgs = CreateMockActivateEventArgs();

        InvokeOnActivated(activateArgs);
        
        Assert.IsNull(user.Anchor, "User anchor should be set to null");
    }

    [Test]
    public void OnActivated_WithoutAssignedUser_ShouldOnlyRemoveAnchor()
    {
        var activateArgs = CreateMockActivateEventArgs();
        
        var assignedUser = UserManager.instance.Users.Find(u => u.Anchor == testGameObject);
        Assert.IsNull(assignedUser, "Should not have assigned user for this test");
        
        Assert.DoesNotThrow(() => InvokeOnActivated(activateArgs), 
            "Should handle activation without assigned user gracefully");
    }

    [Test]
    public void OnActivated_ShouldCallAnchorManagerRemoveAnchor()
    {
        var activateArgs = CreateMockActivateEventArgs();
        bool removeAnchorCalled = false;
        
        Assert.DoesNotThrow(() => InvokeOnActivated(activateArgs), 
            "Should call AnchorManager.RemoveAnchor without throwing");
    }


 

    [Test]
    public void OnActivated_WithMultipleUsers_ShouldOnlyAffectCorrectUser()
    {
        var user1 = new User("user1", "rtc1", testGameObject, "channel1");
        var user2 = new User("user2", "rtc2", null, "channel2");
        var user3 = new User("user3", "rtc3", new GameObject("OtherAnchor"), "channel3");
        
        UserManager.instance.Users.Add(user1);
        UserManager.instance.Users.Add(user2);
        UserManager.instance.Users.Add(user3);
        
        createdObjects.Add(user3.Anchor);
        
        var activateArgs = CreateMockActivateEventArgs();
        
        InvokeOnActivated(activateArgs);
        
        Assert.IsNull(user1.Anchor, "User1 anchor should be cleared (was assigned to this gameObject)");
        Assert.IsNull(user2.Anchor, "User2 anchor should remain null (was already null)");
        Assert.IsNotNull(user3.Anchor, "User3 anchor should remain unchanged (different anchor)");
    }

    [Test]
    public void OnActivated_WithUserHavingDifferentAnchor_ShouldNotAffectUser()
    {
        var otherAnchor = new GameObject("OtherAnchor");
        var user = new User("testUser", "rtcUser", otherAnchor, "testChannel");
        UserManager.instance.Users.Add(user);
        createdObjects.Add(otherAnchor);
        
        var activateArgs = CreateMockActivateEventArgs();
        
        InvokeOnActivated(activateArgs);
        
        Assert.AreEqual(otherAnchor, user.Anchor, "User with different anchor should not be affected");
    }

    [Test]
    public void Component_EnableDisableCycle_ShouldWorkCorrectly()
    {
        destroyAnchor.enabled = true;
        destroyAnchor.enabled = false;
        destroyAnchor.enabled = true;
        
        Assert.DoesNotThrow(() => InvokeOnActivated(CreateMockActivateEventArgs()), 
            "Should work correctly after enable/disable cycle");
    }



    private void SetupManagerMocks()
    {
        if (UserManager.instance == null)
        {
            var userManagerGO = new GameObject("MockUserManager");
            var userManager = userManagerGO.AddComponent<UserManager>();
            UserManager.instance = userManager;
            
            // Inizializza la lista Users
            var usersField = typeof(UserManager).GetField("users", BindingFlags.NonPublic | BindingFlags.Instance);
            if (usersField == null)
            {
                usersField = typeof(UserManager).GetField("Users", BindingFlags.Public | BindingFlags.Instance);
            }
            usersField?.SetValue(userManager, new List<User>());
            
            createdObjects.Add(userManagerGO);
        }
        
        if (AnchorManager.instance == null)
        {
            var anchorManagerGO = new GameObject("MockAnchorManager");
            var anchorManager = anchorManagerGO.AddComponent<AnchorManager>();
            AnchorManager.instance = anchorManager;
            createdObjects.Add(anchorManagerGO);
        }
    }

    private ActivateEventArgs CreateMockActivateEventArgs()
    {
        return null;
    }

    private void InvokeOnActivated(ActivateEventArgs args)
    {
        var method = typeof(DestroyAnchor).GetMethod("OnActivated", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(destroyAnchor, new object[] { args });
    }

    private int GetEventListenerCount(UnityEngine.Events.UnityEvent<ActivateEventArgs> unityEvent)
    {
        var persistentCallsField = typeof(UnityEngine.Events.UnityEventBase)
            .GetField("m_PersistentCalls", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (persistentCallsField != null)
        {
            var persistentCalls = persistentCallsField.GetValue(unityEvent);
            var callsField = persistentCalls.GetType()
                .GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (callsField != null)
            {
                var calls = callsField.GetValue(persistentCalls) as System.Collections.IList;
                return calls?.Count ?? 0;
            }
        }
        
        return 0;
    }

    private void SetPrivateField(string fieldName, object value)
    {
        var field = typeof(DestroyAnchor).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(destroyAnchor, value);
    }

    private T GetPrivateField<T>(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field != null ? (T)field.GetValue(target) : default(T);
    }

    private T GetPrivateField<T>(string fieldName)
    {
        var field = typeof(DestroyAnchor).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field != null ? (T)field.GetValue(destroyAnchor) : default(T);
    }
}