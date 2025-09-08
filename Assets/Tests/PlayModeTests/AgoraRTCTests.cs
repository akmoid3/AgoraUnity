using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;
using Moq;
using Agora.Rtc;

[TestFixture]
public class AgoraRTCManagerTests
{
    private AgoraRTCManager agoraRTCManager;
    private Mock<IRtcEngine> mockRtcEngine;
    private const string TEST_APP_ID = "test_app_id_12345678901234567890";
    private const string TEST_TOKEN = "test_token";
    private const string TEST_CHANNEL_NAME = "test_channel";

    [SetUp]
    public void SetUp()
    {
        mockRtcEngine = new Mock<IRtcEngine>();
        agoraRTCManager = new AgoraRTCManager(TEST_APP_ID, TEST_TOKEN, TEST_CHANNEL_NAME);
    }

    [TearDown]
    public void TearDown()
    {
        agoraRTCManager?.OnDestroy();
        agoraRTCManager = null;
    }

    [Test]
    public void Constructor_ValidParameters_CreatesInstanceSuccessfully()
    {
        // Arrange & Act
        var manager = new AgoraRTCManager("test_app", "test_token", "test_channel");

        // Assert
        Assert.IsNotNull(manager);
    }

    [Test]
    public void GetChannelName_ReturnsCorrectChannelName()
    {
        // Use reflection to access internal method
        var channelName = GetChannelNameViaReflection();
        
        // Assert
        Assert.AreEqual(TEST_CHANNEL_NAME, channelName);
    }

    [Test]
    public void InitEngine_DoesNotThrowException()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => agoraRTCManager.InitEngine());
    }

    [Test]
    public void InitEngine_CreatesRtcEngine()
    {
        // Act
        agoraRTCManager.InitEngine();

        // Assert using reflection
        var rtcEngine = GetRtcEngineViaReflection();
        Assert.IsNotNull(rtcEngine);
    }

    [Test]
    public void JoinChannel_WithMockedEngine_CallsCorrectMethod()
    {
        // Arrange
        uint testUid = 12345;
        SetRtcEngineViaReflection(mockRtcEngine.Object);

        // Act
        agoraRTCManager.JoinChannel(testUid);

        // Assert - Use It.IsAny for optional parameters
        mockRtcEngine.Verify(x => x.JoinChannel(
            It.Is<string>(s => s == TEST_TOKEN), 
            It.Is<string>(s => s == TEST_CHANNEL_NAME), 
            It.Is<string>(s => s == ""), 
            It.Is<uint>(u => u == testUid)), Times.Once);
    }

    [Test]
    public void LeaveChannel_WithMockedEngine_CallsCorrectMethod()
    {
        // Arrange
        SetRtcEngineViaReflection(mockRtcEngine.Object);

        // Act
        agoraRTCManager.LeaveChannel();

        // Assert - Setup the method to return success
        mockRtcEngine.Setup(x => x.LeaveChannel()).Returns(0);
        mockRtcEngine.Verify(x => x.LeaveChannel(), Times.AtLeastOnce);
    }

    [Test]
    public void StartPublish_WithMockedEngine_UpdatesChannelOptions()
    {
        // Arrange
        SetRtcEngineViaReflection(mockRtcEngine.Object);
        mockRtcEngine.Setup(x => x.UpdateChannelMediaOptions(It.IsAny<ChannelMediaOptions>()))
                    .Returns(0);

        // Act
        agoraRTCManager.StartPublish();

        // Assert
        mockRtcEngine.Verify(x => x.UpdateChannelMediaOptions(It.IsAny<ChannelMediaOptions>()), Times.Once);
    }

    [Test]
    public void StopPublish_WithMockedEngine_UpdatesChannelOptions()
    {
        // Arrange
        SetRtcEngineViaReflection(mockRtcEngine.Object);
        mockRtcEngine.Setup(x => x.UpdateChannelMediaOptions(It.IsAny<ChannelMediaOptions>()))
                    .Returns(0);

        // Act
        agoraRTCManager.StopPublish();

        // Assert
        mockRtcEngine.Verify(x => x.UpdateChannelMediaOptions(It.IsAny<ChannelMediaOptions>()), Times.Once);
    }

    [Test]
    public void AdjustVideoEncodedConfiguration640_WithMockedEngine_SetsCorrectConfig()
    {
        // Arrange
        SetRtcEngineViaReflection(mockRtcEngine.Object);
        mockRtcEngine.Setup(x => x.SetVideoEncoderConfiguration(It.IsAny<VideoEncoderConfiguration>()))
                    .Returns(0);

        // Act
        agoraRTCManager.AdjustVideoEncodedConfiguration640();

        // Assert
        mockRtcEngine.Verify(x => x.SetVideoEncoderConfiguration(It.IsAny<VideoEncoderConfiguration>()), Times.Once);
    }

    [Test]
    public void AdjustVideoEncodedConfiguration480_WithMockedEngine_SetsCorrectConfig()
    {
        // Arrange
        SetRtcEngineViaReflection(mockRtcEngine.Object);
        mockRtcEngine.Setup(x => x.SetVideoEncoderConfiguration(It.IsAny<VideoEncoderConfiguration>()))
                    .Returns(0);

        // Act
        agoraRTCManager.AdjustVideoEncodedConfiguration480();

        // Assert
        mockRtcEngine.Verify(x => x.SetVideoEncoderConfiguration(It.IsAny<VideoEncoderConfiguration>()), Times.Once);
    }

    [Test]
    public void OnDestroy_WithMockedEngine_CleansUpCorrectly()
    {
        // Arrange
        SetRtcEngineViaReflection(mockRtcEngine.Object);
        
        // Setup methods to avoid optional parameter issues
        mockRtcEngine.Setup(x => x.InitEventHandler(It.IsAny<IRtcEngineEventHandler>())).Returns(0);
        mockRtcEngine.Setup(x => x.LeaveChannel()).Returns(0);

        // Act
        agoraRTCManager.OnDestroy();

        // Assert - Verify calls were made
        mockRtcEngine.Verify(x => x.InitEventHandler(null), Times.Once);
        mockRtcEngine.Verify(x => x.LeaveChannel(), Times.Once);
        
        // Check that engine is set to null
        var rtcEngine = GetRtcEngineViaReflection();
        Assert.IsNull(rtcEngine);
    }

    [Test]
    public void OnDestroy_WithNullEngine_DoesNotThrow()
    {
        // Arrange
        SetRtcEngineViaReflection(null);

        // Act & Assert
        Assert.DoesNotThrow(() => agoraRTCManager.OnDestroy());
    }

    [Test]
    public void OnApplicationQuit_WithMockedEngine_CleansUpCorrectly()
    {
        // Arrange
        SetRtcEngineViaReflection(mockRtcEngine.Object);
        
        // Setup methods
        mockRtcEngine.Setup(x => x.LeaveChannel()).Returns(0);
        mockRtcEngine.Setup(x => x.InitEventHandler(It.IsAny<IRtcEngineEventHandler>())).Returns(0);

        // Act
        agoraRTCManager.OnApplicationQuit();

        // Assert
        mockRtcEngine.Verify(x => x.LeaveChannel(), Times.Once);
        mockRtcEngine.Verify(x => x.InitEventHandler(null), Times.Once);
        
        // Check that engine is set to null
        var rtcEngine = GetRtcEngineViaReflection();
        Assert.IsNull(rtcEngine);
    }

    [Test]
    public void OnApplicationQuit_WithNullEngine_DoesNotThrow()
    {
        // Arrange
        SetRtcEngineViaReflection(null);

        // Act & Assert
        Assert.DoesNotThrow(() => agoraRTCManager.OnApplicationQuit());
    }

    // Behavioral tests that don't require mocking
    [Test]
    public void JoinChannel_WithValidUid_DoesNotThrowException()
    {
        // Arrange
        agoraRTCManager.InitEngine();
        uint testUid = 12345;

        // Act & Assert
        Assert.DoesNotThrow(() => agoraRTCManager.JoinChannel(testUid));
    }

    [Test]
    public void StartPublish_DoesNotThrowException()
    {
        // Arrange
        agoraRTCManager.InitEngine();

        // Act & Assert
        Assert.DoesNotThrow(() => agoraRTCManager.StartPublish());
    }

    [Test]
    public void StopPublish_DoesNotThrowException()
    {
        // Arrange
        agoraRTCManager.InitEngine();

        // Act & Assert
        Assert.DoesNotThrow(() => agoraRTCManager.StopPublish());
    }

    [Test]
    public void MethodsSequence_InitJoinLeaveDestroy_WorksCorrectly()
    {
        // Test a complete sequence of operations
        Assert.DoesNotThrow(() => {
            agoraRTCManager.InitEngine();
            agoraRTCManager.JoinChannel(12345);
            agoraRTCManager.StartPublish();
            agoraRTCManager.AdjustVideoEncodedConfiguration640();
            agoraRTCManager.StopPublish();
            agoraRTCManager.LeaveChannel();
            agoraRTCManager.OnDestroy();
        });
    }

    // Helper methods using reflection to access internal members
    private string GetChannelNameViaReflection()
    {
        var method = typeof(AgoraRTCManager).GetMethod("GetChannelName", 
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        return (string)method?.Invoke(agoraRTCManager, null);
    }

    private object GetRtcEngineViaReflection()
    {
        var field = typeof(AgoraRTCManager).GetField("RtcEngine", 
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        return field?.GetValue(agoraRTCManager);
    }

    private void SetRtcEngineViaReflection(IRtcEngine engine)
    {
        var field = typeof(AgoraRTCManager).GetField("RtcEngine", 
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        field?.SetValue(agoraRTCManager, engine);
    }
}