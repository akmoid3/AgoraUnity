using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture]
public class UserManagerTests
{
    private UserManager userManager;
    private GameObject testGameObject;
    private GameObject mockUserPrefab;
    private GameObject anchorManagerGO;

    [SetUp]
    public void SetUp()
    {
        anchorManagerGO = new GameObject("MockAnchorManager");
        var anchorManager = anchorManagerGO.AddComponent<AnchorManager>();
        AnchorManager.instance = anchorManager;
        // Crea un GameObject per il test
        testGameObject = new GameObject();
        userManager = testGameObject.AddComponent<UserManager>();
        
        // Crea un mock del user prefab
        mockUserPrefab = new GameObject("MockUserPrefab");
        userManager.UserPrefab = mockUserPrefab;
        
        // Reset singleton instance
        UserManager.instance = null;
        
        // Simula Start()
        userManager.Start();
    }

    [TearDown]
    public void TearDown()
    {
        if (testGameObject != null)
            Object.DestroyImmediate(testGameObject);
        if (mockUserPrefab != null)
            Object.DestroyImmediate(mockUserPrefab);
        if(anchorManagerGO != null)
            Object.DestroyImmediate(anchorManagerGO);
        AnchorManager.instance = null;
        UserManager.instance = null;
    }

    [Test]
    public void Start_ShouldSetSingletonInstance()
    {
        // Assert
        Assert.AreEqual(userManager, UserManager.instance);
    }

    [Test]
    public void Start_WithExistingInstance_ShouldDestroyNewInstance()
    {
        // Arrange
        var firstInstance = UserManager.instance;
        var secondGameObject = new GameObject();
        var secondUserManager = secondGameObject.AddComponent<UserManager>();
        
        // Act
        secondUserManager.Start();
        
        // Assert
        Assert.AreEqual(firstInstance, UserManager.instance);
        
        // Cleanup
        Object.DestroyImmediate(secondGameObject);
    }

    [Test]
    public void AddUser_ShouldAddUserToList()
    {
        // Arrange
        var mockUser = new User("user1", "123", null, "prova");

        
        // Act
        userManager.AddUser(mockUser);
        
        // Assert
        Assert.Contains(mockUser, userManager.Users);
        Assert.AreEqual(1, userManager.Users.Count);
    }

    [Test]
    public void AddUser_ShouldCallAnchorManagerAssignAnchor()
    {
        // Arrange
        var mockUser = new User("user1", "123", null, "prova");

        // Act
        userManager.AddUser(mockUser);
        
        // Assert
        // Verifica che l'utente sia stato aggiunto
        Assert.Contains(mockUser, userManager.Users);
    }

    [Test]
    public void RemoveUser_ShouldRemoveUserFromBothLists()
    {
        // Arrange
        var mockUser = new User("user1", "123", null, "prova");

        userManager.Users.Add(mockUser);
        userManager.WaitingUsers.Add(mockUser);
        
        // Act
        userManager.RemoveUser(mockUser);
        
        // Assert
        Assert.IsFalse(userManager.Users.Contains(mockUser));
        Assert.IsFalse(userManager.WaitingUsers.Contains(mockUser));
    }

    [Test]
    public void AddWaitingUser_ShouldAddUserToWaitingList()
    {
        // Arrange
        var mockUser = new User("user1", "123", null, "prova");

        
        // Act
        userManager.AddWaitingUser(mockUser);
        
        // Assert
        Assert.Contains(mockUser, userManager.WaitingUsers);
        Assert.AreEqual(1, userManager.WaitingUsers.Count);
    }

    [Test]
    public void AddWaitingUser_WithDuplicateUser_ShouldNotAddDuplicate()
    {
        // Arrange
        var mockUser = new User("user1", "123", null, "prova");

        userManager.WaitingUsers.Add(mockUser);
        
        // Act
        userManager.AddWaitingUser(mockUser);
        
        // Assert
        Assert.AreEqual(1, userManager.WaitingUsers.Count);
    }

    [Test]
    public void RemoveWaitingUser_ShouldRemoveUserFromWaitingList()
    {
        // Arrange
        var mockUser = new User("user1", "123", null, "prova");
        
        userManager.WaitingUsers.Add(mockUser);
        
        // Act
        userManager.RemoveWaitingUser(mockUser);
        
        // Assert
        Assert.IsFalse(userManager.WaitingUsers.Contains(mockUser));
        Assert.AreEqual(0, userManager.WaitingUsers.Count);
    }

    [Test]
    public void OnCallQuit_ShouldClearAllLists()
    {
        // Arrange
        var user1 = new User("user1", "123", null, "prova");

        var user2 = new User("user2", "321", null, "prova");
        userManager.Users.Add(user1);
        userManager.WaitingUsers.Add(user2);
        
        // Act
        userManager.OnCallQuit();
        
        // Assert
        Assert.AreEqual(0, userManager.Users.Count);
        Assert.AreEqual(0, userManager.WaitingUsers.Count);
    }

    [Test]
    public void GetUser_WithValidUID_ShouldReturnCorrectUser()
    {
        // Arrange
        var user1 = new User("user1", "123", null, "prova");

        var user2 = new User("user2", "321", null, "prova");

        userManager.Users.Add(user1);
        userManager.Users.Add(user2);
        
        // Act
        var result = userManager.GetUser("123");
        
        // Assert
        Assert.AreEqual(user1, result);
    }

    [Test]
    public void GetUser_WithInvalidUID_ShouldReturnNull()
    {
        // Arrange
        var user1 = new User("user1", "123", null, "prova");
        userManager.Users.Add(user1);
        
        // Act
        var result = userManager.GetUser("nonexistent");
        
        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void GetUser_WithEmptyUsersList_ShouldReturnNull()
    {
        // Act
        var result = userManager.GetUser("anyid");
        
        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void Properties_ShouldGetAndSetCorrectly()
    {
        // Arrange
        var newUsersList = new List<User>();
        var newWaitingUsersList = new List<User>();
        var newPrefab = new GameObject("NewPrefab");
        
        // Act
        userManager.Users = newUsersList;
        userManager.WaitingUsers = newWaitingUsersList;
        userManager.UserPrefab = newPrefab;
        
        // Assert
        Assert.AreEqual(newUsersList, userManager.Users);
        Assert.AreEqual(newWaitingUsersList, userManager.WaitingUsers);
        Assert.AreEqual(newPrefab, userManager.UserPrefab);
        
        // Cleanup
        Object.DestroyImmediate(newPrefab);
    }
    
}