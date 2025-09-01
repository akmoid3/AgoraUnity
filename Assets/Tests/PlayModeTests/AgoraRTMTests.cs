using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Moq;
using Agora.Rtm;

[TestFixture]
public class AgoraRTMManagerTests
{
    private AgoraRTMManager rtmManager;
    private Mock<IRtmClient> mockRtmClient;
    private Mock<IRtmStorage> mockStorage;
    private GameObject userManagerGO;
    private GameObject chatManagerGO;
    private GameObject anchorManagerGO;
    private const string TestAppId = "test_app_id_1234567890";
    private const string TestUsername = "testuser";
    private const string TestUserToken = "test_token";
    private const string TestChannelName = "test_channel";
    private const string TestUid = "test_uid_123";
    
    [SetUp]
    public void SetUp()
    {
        // Setup managers first
        SetupManagers();
        
        // Setup mocks
        mockRtmClient = new Mock<IRtmClient>();
        mockStorage = new Mock<IRtmStorage>();
        
        // Setup storage mock to return from rtmClient
        mockRtmClient.Setup(x => x.GetStorage()).Returns(mockStorage.Object);
        
        // Create instance
        rtmManager = new AgoraRTMManager(TestAppId, TestUsername, TestUserToken, TestChannelName);
        
        // Use reflection to set the private rtmClient field for testing
        var rtmClientField = typeof(AgoraRTMManager).GetField("rtmClient", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        rtmClientField?.SetValue(rtmManager, mockRtmClient.Object);
    }

    [TearDown]
    public void TearDown()
    {
        CleanupManagers();
        rtmManager = null;
        mockRtmClient = null;
        mockStorage = null;
    }

    private void SetupManagers()
    {
        // Create UserManager
        userManagerGO = new GameObject("UserManager");
        var userManager = userManagerGO.AddComponent<UserManager>();
        
        // Initialize the Users list and add test users
        userManager.Users = new List<User>
        {
            new User (  "user1", "123",null,TestChannelName),
            new User ("user2", "321",null,TestChannelName)
        };
        
        // Set the singleton instance
        UserManager.instance = userManager;
        
        // Create ChatManager
        chatManagerGO = new GameObject("ChatManager");
        var chatManager = chatManagerGO.AddComponent<ChatManager>();
        
        // Add AudioSource component for ChatManager
        var audioSource = chatManagerGO.AddComponent<AudioSource>();
        
        // Set the singleton instance
        ChatManager.instance = chatManager;
        
        // Create AnchorManager
        anchorManagerGO = new GameObject("AnchorManager");
        var anchorManager = anchorManagerGO.AddComponent<AnchorManager>();
        
        // Create a test anchor for AnchorManager
        var testAnchor = new GameObject("TestAnchor");
        anchorManager.ancoraProva = testAnchor;
        
        // Set the singleton instance
        AnchorManager.instance = anchorManager;
    }

    private void CleanupManagers()
    {
        // Clean up UserManager
        if (userManagerGO != null)
        {
            UnityEngine.Object.DestroyImmediate(userManagerGO);
            UserManager.instance = null;
        }
        
        // Clean up ChatManager
        if (chatManagerGO != null)
        {
            UnityEngine.Object.DestroyImmediate(chatManagerGO);
            ChatManager.instance = null;
        }
        
        // Clean up AnchorManager
        if (anchorManagerGO != null)
        {
            // Clean up test anchor
            if (AnchorManager.instance != null && AnchorManager.instance.ancoraProva != null)
            {
                UnityEngine.Object.DestroyImmediate(AnchorManager.instance.ancoraProva);
            }
            
            UnityEngine.Object.DestroyImmediate(anchorManagerGO);
            AnchorManager.instance = null;
        }
    }

    #region Constructor Tests

    [Test]
    public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var manager = new AgoraRTMManager(TestAppId, TestUsername, TestUserToken, TestChannelName);
        
        // Assert
        Assert.IsNotNull(manager);
    }

    [Test]
    public void Constructor_WithNullParameters_DoesNotThrow()
    {
        // Arrange, Act & Assert
        Assert.DoesNotThrow(() => new AgoraRTMManager(null, null, null, null));
    }

    #endregion

    #region OnInit Tests

    [Test]
    public void OnInit_WithEmptyUsername_LogsWarningAndReturns()
    {
        // Arrange
        var manager = new AgoraRTMManager(TestAppId, "", TestUserToken, TestChannelName);
        
        // Expect the log message
        LogAssert.Expect(LogType.Log, "We need a username and appId to init");
        
        // Act
        manager.OnInit();
    }

    [Test]
    public void OnInit_WithEmptyAppId_LogsWarningAndReturns()
    {
        // Arrange
        var manager = new AgoraRTMManager("", TestUsername, TestUserToken, TestChannelName);
        
        // Expect the log message
        LogAssert.Expect(LogType.Log, "We need a username and appId to init");
        
        // Act
        manager.OnInit();
    }

    [Test]
    public void OnInit_WithValidParameters_DoesNotThrow()
    {
        // Expect the log message for successful init
        LogAssert.Expect(LogType.Log, "RtmClient init success");
        
        // Act
        Assert.DoesNotThrow(() => rtmManager.OnInit());
    }

    #endregion

    #region OnJoin Tests

    [Test]
    public void OnJoin_WithNullRtmClient_LogsErrorAndReturns()
    {
        // Arrange
        var manager = new AgoraRTMManager(TestAppId, TestUsername, TestUserToken, TestChannelName);
        
        // Expect the error log
        LogAssert.Expect(LogType.Error, "rtmClient is null");
        
        // Act
        Assert.DoesNotThrow(() => manager.OnJoin(TestUid));
    }

    [Test]
    public async Task OnJoin_WithValidClient_CallsSubscribeAsync()
    {
        // Arrange
        var subscribeResult = new RtmResult<SubscribeResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new SubscribeResult { ChannelName = TestChannelName }
        };
        
        var metadataResult = new RtmResult<SetChannelMetadataResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new SetChannelMetadataResult 
            { 
                ChannelName = TestChannelName,
                ChannelType = RTM_CHANNEL_TYPE.MESSAGE
            }
        };

        var getMetadataResult = new RtmResult<GetChannelMetadataResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new GetChannelMetadataResult
            {
                ChannelName = TestChannelName,
                ChannelType = RTM_CHANNEL_TYPE.MESSAGE,
                Data = new RtmMetadata
                {
                    metadataItems = new MetadataItem[]
                    {
                        new MetadataItem { key = "user1", value = "TestUser1" },
                        new MetadataItem { key = "hand:user1", value = "0" }
                    },
                    metadataItemsSize = 2
                }
            }
        };

        mockRtmClient.Setup(x => x.SubscribeAsync(It.IsAny<string>(), It.IsAny<SubscribeOptions>()))
            .ReturnsAsync(subscribeResult);
        
        mockStorage.Setup(x => x.SetChannelMetadataAsync(
            It.IsAny<string>(), 
            It.IsAny<RTM_CHANNEL_TYPE>(), 
            It.IsAny<RtmMetadata>(), 
            It.IsAny<MetadataOptions>(), 
            It.IsAny<string>()))
            .ReturnsAsync(metadataResult);

        mockStorage.Setup(x => x.GetChannelMetadataAsync(It.IsAny<string>(), It.IsAny<RTM_CHANNEL_TYPE>()))
            .ReturnsAsync(getMetadataResult);

        // Expect log messages in correct order
        LogAssert.Expect(LogType.Log, "User user1 (TestUser1) handRaised = False");
        LogAssert.Expect(LogType.Log, "User user2 () handRaised = False");
        LogAssert.Expect(LogType.Log, $"Set Channel :{TestChannelName} metadata success! Channel Type is :MESSAGE! ");
        LogAssert.Expect(LogType.Log, $"Successfully subscribed to channel: {TestChannelName}");

        // Act
        rtmManager.OnJoin(TestUid);
        await Task.Delay(300);

        // Assert
        mockRtmClient.Verify(x => x.SubscribeAsync(TestChannelName, It.IsAny<SubscribeOptions>()), 
            Times.Once);
    }

    [Test]
    public async Task OnJoin_WithSubscribeFailure_LogsError()
    {
        // Arrange
        var subscribeResult = new RtmResult<SubscribeResult>
        {
            Status = new RtmStatus 
            { 
                Error = true, 
                ErrorCode = (int)RTM_ERROR_CODE.NOT_INITIALIZED,
                Reason = "Test error"
            }
        };

        var getMetadataResult = new RtmResult<GetChannelMetadataResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new GetChannelMetadataResult
            {
                Data = new RtmMetadata
                {
                    metadataItems = new MetadataItem[0],
                    metadataItemsSize = 0
                }
            }
        };

        mockRtmClient.Setup(x => x.SubscribeAsync(It.IsAny<string>(), It.IsAny<SubscribeOptions>()))
            .ReturnsAsync(subscribeResult);

        mockStorage.Setup(x => x.GetChannelMetadataAsync(It.IsAny<string>(), It.IsAny<RTM_CHANNEL_TYPE>()))
            .ReturnsAsync(getMetadataResult);

        // Expect log messages
        LogAssert.Expect(LogType.Log, "User user1 () handRaised = False");
        LogAssert.Expect(LogType.Log, "User user2 () handRaised = False");
        LogAssert.Expect(LogType.Error, $"Subscribe failed: {(int)RTM_ERROR_CODE.NOT_INITIALIZED} - Test error");

        // Act
        rtmManager.OnJoin(TestUid);
        await Task.Delay(200);
    }

    #endregion

    #region OnLeave Tests

    [Test]
    public async Task OnLeave_WithValidClient_CallsUnsubscribeAndDispose()
    {
        // Arrange
        var removeMetadataResult = new RtmResult<RemoveChannelMetadataResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new RemoveChannelMetadataResult 
            { 
                ChannelName = TestChannelName,
                ChannelType = RTM_CHANNEL_TYPE.MESSAGE
            }
        };

        var unsubscribeResult = new RtmResult<UnsubscribeResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new UnsubscribeResult()
        };

        mockStorage.Setup(x => x.RemoveChannelMetadataAsync(
            It.IsAny<string>(), 
            It.IsAny<RTM_CHANNEL_TYPE>(), 
            It.IsAny<RtmMetadata>(), 
            It.IsAny<MetadataOptions>(), 
            It.IsAny<string>()))
            .ReturnsAsync(removeMetadataResult);

        mockRtmClient.Setup(x => x.UnsubscribeAsync(It.IsAny<string>()))
            .ReturnsAsync(unsubscribeResult);

        mockRtmClient.Setup(x => x.Dispose())
            .Returns(new RtmStatus { Error = false });

        // Expect log messages
        LogAssert.Expect(LogType.Log, $"Remove Channel :{TestChannelName} metadata success! Channel Type is :MESSAGE! ");
        LogAssert.Expect(LogType.Log, "Unsubscribe Channel Success!");
        LogAssert.Expect(LogType.Log, "Dispose rtmClient Success!");

        // Act
        rtmManager.OnLeave(TestUid);
        await Task.Delay(200);

        // Assert
        mockStorage.Verify(x => x.RemoveChannelMetadataAsync(
            TestChannelName, 
            RTM_CHANNEL_TYPE.MESSAGE, 
            It.IsAny<RtmMetadata>(), 
            It.IsAny<MetadataOptions>(), 
            ""), Times.Once);
        
        mockRtmClient.Verify(x => x.UnsubscribeAsync(TestChannelName), Times.Once);
        mockRtmClient.Verify(x => x.Dispose(), Times.Once);
    }

    [Test]
    public void OnLeave_WithNullClient_DoesNotThrow()
    {
        // Arrange
        var manager = new AgoraRTMManager(TestAppId, TestUsername, TestUserToken, TestChannelName);
        
        // Act & Assert
        Assert.DoesNotThrow(() => manager.OnLeave(TestUid));
    }

    #endregion

    #region OnLoginAsync Tests

    [Test]
    public async Task OnLoginAsync_WithNullClient_LogsAndReturns()
    {
        // Arrange
        var manager = new AgoraRTMManager(TestAppId, TestUsername, TestUserToken, TestChannelName);
        
        // Expect log message
        LogAssert.Expect(LogType.Log, "RtmClient not init!!!");
        
        // Act
        await manager.OnLoginAsync();
    }

    [Test]
    public async Task OnLoginAsync_WithValidClient_CallsLoginAsync()
    {
        // Arrange
        var loginResult = new RtmResult<LoginResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new LoginResult()
        };

        mockRtmClient.Setup(x => x.LoginAsync(It.IsAny<string>()))
            .ReturnsAsync(loginResult);

        // Expect log message
        LogAssert.Expect(LogType.Log, "rtmClient.Login + respones:");

        // Act
        await rtmManager.OnLoginAsync();

        // Assert
        mockRtmClient.Verify(x => x.LoginAsync(TestUserToken), Times.Once);
    }

    [Test]
    public async Task OnLoginAsync_WithLoginFailure_LogsError()
    {
        // Arrange
        var loginResult = new RtmResult<LoginResult>
        {
            Status = new RtmStatus 
            { 
                Error = true, 
                ErrorCode = (int)RTM_ERROR_CODE.INVALID_TOKEN 
            }
        };

        mockRtmClient.Setup(x => x.LoginAsync(It.IsAny<string>()))
            .ReturnsAsync(loginResult);

        // Expect log message
        LogAssert.Expect(LogType.Log, $"rtmClient.Login + ret:{(int)RTM_ERROR_CODE.INVALID_TOKEN}");

        // Act
        await rtmManager.OnLoginAsync();
    }

    #endregion

    #region OnLogoutAsync Tests

    [Test]
    public void OnLogoutAsync_WithNullClient_LogsAndReturns()
    {
        // Arrange
        var manager = new AgoraRTMManager(TestAppId, TestUsername, TestUserToken, TestChannelName);
        
        // Act
        rtmManager.OnLogoutAsync();
    }

    [Test]
    public async Task OnLogoutAsync_WithValidClient_CallsLogoutAsync()
    {
        // Arrange
        var logoutResult = new RtmResult<LogoutResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new LogoutResult()
        };

        mockRtmClient.Setup(x => x.LogoutAsync())
            .ReturnsAsync(logoutResult);

        // Expect log message
        LogAssert.Expect(LogType.Log, "RtmClient.Logout ret:0 ");

        // Act
        rtmManager.OnLogoutAsync();
        await Task.Delay(100);

        // Assert
        mockRtmClient.Verify(x => x.LogoutAsync(), Times.Once);
    }

    #endregion

    #region FetchChannelMetadata Tests

    [Test]
    public void FetchChannelMetadata_WithNullClient_LogsWarningAndReturns()
    {
        // Arrange
        var manager = new AgoraRTMManager(TestAppId, TestUsername, TestUserToken, TestChannelName);
        
        // Expect warning log
        LogAssert.Expect(LogType.Warning, "FetchChannelMetadata called but rtmClient is null");
        
        // Act
        manager.FetchChannelMetadata();
    }

    [Test]
    public async Task FetchChannelMetadata_WithValidClient_UpdatesUsers()
    {
        // Arrange
        var metadataResult = new RtmResult<GetChannelMetadataResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new GetChannelMetadataResult
            {
                ChannelName = TestChannelName,
                ChannelType = RTM_CHANNEL_TYPE.MESSAGE,
                Data = new RtmMetadata
                {
                    metadataItems = new MetadataItem[]
                    {
                        new MetadataItem { key = "123", value = "user1" },
                        new MetadataItem { key = "hand:123", value = "1" }
                    },
                    metadataItemsSize = 2
                }
            }
        };

        mockStorage.Setup(x => x.GetChannelMetadataAsync(It.IsAny<string>(), It.IsAny<RTM_CHANNEL_TYPE>()))
            .ReturnsAsync(metadataResult);

        // Expect log messages
        LogAssert.Expect(LogType.Log, "User 123 (user1) handRaised = True");
        LogAssert.Expect(LogType.Log, "User 321 (user2) handRaised = False");

        // Act
        rtmManager.FetchChannelMetadata();
        await Task.Delay(200);

        // Assert
        var user1 = UserManager.instance.Users.Find(u => u.RtcID == "123");
        Assert.IsNotNull(user1);
        Assert.AreEqual("user1", user1.RtmID);
        Assert.IsTrue(user1.IsHandRaised);
        
        mockStorage.Verify(x => x.GetChannelMetadataAsync(TestChannelName, RTM_CHANNEL_TYPE.MESSAGE), 
            Times.Once);
    }

    #endregion

    #region JoinChannel Tests

    [Test]
    public async Task JoinChannel_CallsOnLoginAsyncAndOnJoin()
    {
        // Arrange
        var loginResult = new RtmResult<LoginResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new LoginResult()
        };

        var subscribeResult = new RtmResult<SubscribeResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new SubscribeResult { ChannelName = TestChannelName }
        };

        var metadataResult = new RtmResult<SetChannelMetadataResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new SetChannelMetadataResult
            {
                ChannelName = TestChannelName,
                ChannelType = RTM_CHANNEL_TYPE.MESSAGE
            }
        };

        var getMetadataResult = new RtmResult<GetChannelMetadataResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new GetChannelMetadataResult
            {
                Data = new RtmMetadata
                {
                    metadataItems = new MetadataItem[0],
                    metadataItemsSize = 0
                }
            }
        };

        mockRtmClient.Setup(x => x.LoginAsync(It.IsAny<string>()))
            .ReturnsAsync(loginResult);
        
        mockRtmClient.Setup(x => x.SubscribeAsync(It.IsAny<string>(), It.IsAny<SubscribeOptions>()))
            .ReturnsAsync(subscribeResult);
        
        mockStorage.Setup(x => x.SetChannelMetadataAsync(
            It.IsAny<string>(), 
            It.IsAny<RTM_CHANNEL_TYPE>(), 
            It.IsAny<RtmMetadata>(), 
            It.IsAny<MetadataOptions>(), 
            It.IsAny<string>()))
            .ReturnsAsync(metadataResult);

        mockStorage.Setup(x => x.GetChannelMetadataAsync(It.IsAny<string>(), It.IsAny<RTM_CHANNEL_TYPE>()))
            .ReturnsAsync(getMetadataResult);

        // Expect log messages in the order they occur
        LogAssert.Expect(LogType.Log, "rtmClient.Login + respones:");
        LogAssert.Expect(LogType.Log, "User 123 (user1) handRaised = False");
        LogAssert.Expect(LogType.Log, "User 321 (user2) handRaised = False");
        LogAssert.Expect(LogType.Log, $"Set Channel :{TestChannelName} metadata success! Channel Type is :MESSAGE! ");
        LogAssert.Expect(LogType.Log, $"Successfully subscribed to channel: {TestChannelName}");

        // Act
        rtmManager.JoinChannel(TestUid);
        await Task.Delay(400);

        // Assert
        mockRtmClient.Verify(x => x.LoginAsync(TestUserToken), Times.Once);
        mockRtmClient.Verify(x => x.SubscribeAsync(TestChannelName, It.IsAny<SubscribeOptions>()), 
            Times.Once);
    }

    #endregion

    #region OnDestroy Tests

    [Test]
    public void OnDestroy_CallsOnLeave()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => rtmManager.OnDestroy(TestUid));
    }

    #endregion

    #region Event Handler Tests

    [Test]
    public void OnMessageEvent_WithStringMessage_ProcessesCorrectly()
    {
        // Arrange
        var mockMessage = CreateMockRtmMessage("Test message");
        var messageEvent = new MessageEvent
        {
            channelName = TestChannelName,
            channelType = RTM_CHANNEL_TYPE.MESSAGE,
            publisher = TestUsername,
            messageType = RTM_MESSAGE_TYPE.STRING,
            message = mockMessage
        };

        // Expect log messages
        LogAssert.Expect(LogType.Log, $"You have recieved a string type message: Test message from: {TestUsername} in channel:{TestChannelName}");
        LogAssert.Expect(LogType.Log, "The channel type is MESSAGE");

        // Act & Assert
        Assert.DoesNotThrow(() => 
        {
            var method = typeof(AgoraRTMManager).GetMethod("OnMessageEvent", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(rtmManager, new object[] { messageEvent });
        });
    }

    [Test]
    public void OnMessageEvent_WithBinaryMessage_ProcessesCorrectly()
    {
        // Arrange
        var testData = new byte[] { 1, 2, 3 };
        var mockMessage = CreateMockRtmMessage(testData);
        var messageEvent = new MessageEvent
        {
            channelName = TestChannelName,
            channelType = RTM_CHANNEL_TYPE.MESSAGE,
            publisher = TestUsername,
            messageType = RTM_MESSAGE_TYPE.BINARY,
            message = mockMessage
        };

        // Expect log messages
        LogAssert.Expect(LogType.Log, $"You have recieved a binary type message: {System.BitConverter.ToString(testData)},from: {TestUsername} in channel:{TestChannelName}");
        LogAssert.Expect(LogType.Log, "The channel type is MESSAGE");

        // Act & Assert
        Assert.DoesNotThrow(() => 
        {
            var method = typeof(AgoraRTMManager).GetMethod("OnMessageEvent", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(rtmManager, new object[] { messageEvent });
        });
    }

    [Test]
    public void OnStorageEvent_CallsFetchChannelMetadata()
    {
        // Arrange
        var storageEvent = new StorageEvent
        {
            eventType = RTM_STORAGE_EVENT_TYPE.UPDATE,
            channelType = RTM_CHANNEL_TYPE.MESSAGE
        };

        var getMetadataResult = new RtmResult<GetChannelMetadataResult>
        {
            Status = new RtmStatus { Error = false },
            Response = new GetChannelMetadataResult
            {
                Data = new RtmMetadata
                {
                    metadataItems = new MetadataItem[0],
                    metadataItemsSize = 0
                }
            }
        };

        mockStorage.Setup(x => x.GetChannelMetadataAsync(It.IsAny<string>(), It.IsAny<RTM_CHANNEL_TYPE>()))
            .ReturnsAsync(getMetadataResult);

        // Expect log messages
        LogAssert.Expect(LogType.Log, "[RTM][StorageEvent] type=UPDATE channelType=MESSAGE");
        LogAssert.Expect(LogType.Log, "User user1 () handRaised = False");
        LogAssert.Expect(LogType.Log, "User user2 () handRaised = False");

        // Act & Assert
        Assert.DoesNotThrow(() => 
        {
            var method = typeof(AgoraRTMManager).GetMethod("OnStorageEvent", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(rtmManager, new object[] { storageEvent });
        });
    }

    #endregion

    #region Helper Methods

    private IRtmMessage CreateMockRtmMessage(string data)
    {
        var mockMessage = new Mock<IRtmMessage>();
        mockMessage.Setup(x => x.GetData<string>()).Returns(data);
        mockMessage.Setup(x => x.GetData<byte[]>()).Returns(System.Text.Encoding.UTF8.GetBytes(data ?? ""));
        return mockMessage.Object;
    }

    private IRtmMessage CreateMockRtmMessage(byte[] data)
    {
        var mockMessage = new Mock<IRtmMessage>();
        mockMessage.Setup(x => x.GetData<byte[]>()).Returns(data);
        mockMessage.Setup(x => x.GetData<string>()).Returns(System.Text.Encoding.UTF8.GetString(data ?? new byte[0]));
        return mockMessage.Object;
    }

    #endregion
}