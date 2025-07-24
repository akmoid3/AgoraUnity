using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;
using System.Collections.Generic;

public class AgoraRTCManager : MonoBehaviour
{
    [Header("_____________Basic Configuration_____________")] [SerializeField]
    private string appID = "";

    [SerializeField] private string token = "";

    [SerializeField] private string channelName = "";

    internal IRtcEngine RtcEngine = null;

    private DeviceInfo[] _videoDeviceInfos;


    // Use this for initialization
    private void Start()
    {
        if (CheckAppId())
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            InitEngine();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        PermissionHelper.RequestMicrophontPermission();
        //PermissionHelper.RequestCameraPermission();
    }


    private bool CheckAppId()
    {
        return appID != null && appID != "" && appID.Length > 10;
    }


    #region -- Button Events ---

    public void InitEngine()
    {
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngineContext context = new RtcEngineContext();
        context.appId = appID;
        context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
        context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;

        RtcEngine.Initialize(context);

        RtcEngine.InitEventHandler(handler);


        RtcEngine.EnableAudio();
        RtcEngine.EnableVideo();
        VideoEncoderConfiguration config = new VideoEncoderConfiguration();
        config.dimensions = new VideoDimensions(640, 360);
        config.frameRate = 15;
        config.bitrate = 0;
        RtcEngine.SetVideoEncoderConfiguration(config);
        RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
    }

    public void JoinChannel()
    {
        RtcEngine.JoinChannel(token, channelName, "", 0);
        var node = MakeVideoView(0);
        //CreateLocalVideoCallQualityPanel(node);
    }

    public void LeaveChannel()
    {
        RtcEngine.LeaveChannel();
    }

    /*public void StartPreview()
    {
        RtcEngine.StartPreview();
        var node = MakeVideoView(0);
        CreateLocalVideoCallQualityPanel(node);
    }

    public void StopPreview()
    {
        DestroyVideoView(0);
        RtcEngine.StopPreview();
    }*/

    public void StartPublish()
    {
        var options = new ChannelMediaOptions();
        options.publishMicrophoneTrack.SetValue(true);
        var nRet = RtcEngine.UpdateChannelMediaOptions(options);
        Debug.Log("UpdateChannelMediaOptions: " + nRet);
    }

    public void StopPublish()
    {
        var options = new ChannelMediaOptions();
        options.publishMicrophoneTrack.SetValue(false);
        var nRet = RtcEngine.UpdateChannelMediaOptions(options);
        Debug.Log("UpdateChannelMediaOptions: " + nRet);
    }

    public void AdjustVideoEncodedConfiguration640()
    {
        VideoEncoderConfiguration config = new VideoEncoderConfiguration();
        config.dimensions = new VideoDimensions(640, 360);
        config.frameRate = 15;
        config.bitrate = 0;
        RtcEngine.SetVideoEncoderConfiguration(config);
    }

    public void AdjustVideoEncodedConfiguration480()
    {
        VideoEncoderConfiguration config = new VideoEncoderConfiguration();
        config.dimensions = new VideoDimensions(480, 480);
        config.frameRate = 15;
        config.bitrate = 0;
        RtcEngine.SetVideoEncoderConfiguration(config);
    }

    /*public void GetVideoDeviceManager()
    {
        _videoDeviceSelect.ClearOptions();

        _videoDeviceManager = RtcEngine.GetVideoDeviceManager();
        _videoDeviceInfos = _videoDeviceManager.EnumerateVideoDevices();
        Log.UpdateLog(string.Format("VideoDeviceManager count: {0}", _videoDeviceInfos.Length));
        for (var i = 0; i < _videoDeviceInfos.Length; i++)
        {
            Log.UpdateLog(string.Format("VideoDeviceManager device index: {0}, name: {1}, id: {2}", i,
                _videoDeviceInfos[i].deviceName, _videoDeviceInfos[i].deviceId));
        }

        _videoDeviceSelect.AddOptions(_videoDeviceInfos.Select(w =>
                new Dropdown.OptionData(
                    string.Format("{0} :{1}", w.deviceName, w.deviceId)))
            .ToList());
    }

    public void SelectVideoCaptureDevice()
    {
        if (_videoDeviceSelect == null) return;
        var option = _videoDeviceSelect.options[_videoDeviceSelect.value].text;
        if (string.IsNullOrEmpty(option)) return;

        var deviceId = option.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
        var ret = _videoDeviceManager.SetDevice(deviceId);
        Log.UpdateLog("SelectVideoCaptureDevice ret:" + ret + " , DeviceId: " + deviceId);
    }*/

    #endregion

    private void OnDestroy()
    {
        Debug.Log("OnDestroy");
        if (RtcEngine == null) return;
        RtcEngine.InitEventHandler(null);
        RtcEngine.LeaveChannel();
        RtcEngine.Dispose();
        RtcEngine = null;
    }

    void OnApplicationQuit()
    {
        if (RtcEngine != null)
        {
            RtcEngine.LeaveChannel();
            RtcEngine.InitEventHandler(null);
            RtcEngine.Dispose();
            RtcEngine = null;
        }
    }

    internal string GetChannelName()
    {
        return channelName;
    }

    #region -- Video Render UI Logic ---

    internal GameObject MakeVideoView(uint uid, string channelId = "")
    {
        var go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            return go; // reuse
        }

        // create a GameObject and assign to this new user
        //var videoSurface = MakeImageSurface(uid.ToString());
        var videoSurface = MakePlaneSurface(uid.ToString());

        if (ReferenceEquals(videoSurface, null)) return null;
        // configure videoSurface
        if (uid == 0)
        {
            videoSurface.SetForUser(uid, channelId);
        }
        else
        {
            videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        }

        videoSurface.OnTextureSizeModify += (int width, int height) =>
        {
            var transform = videoSurface.GetComponent<RectTransform>();
            if (transform)
            {
                //If render in RawImage. just set rawImage size.
                transform.sizeDelta = new Vector2(width / 2, height / 2);
                transform.localScale = Vector3.one;
            }
            else
            {
                //If render in MeshRenderer, just set localSize with MeshRenderer
                float scale = (float)height / (float)width;
                videoSurface.transform.localScale = new Vector3(-1, 1, scale);
            }

            Debug.Log("OnTextureSizeModify: " + width + "  " + height);
        };

        videoSurface.SetEnable(true);
        return videoSurface.gameObject;
    }

    // VIDEO TYPE 1: 3D Object
    private static VideoSurface MakePlaneSurface(string goName)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);

        if (go == null)
        {
            return null;
        }

        go.name = goName;
        var mesh = go.GetComponent<MeshRenderer>();
        if (mesh != null)
        {
            Debug.LogWarning("VideoSureface update shader");
            mesh.material = new Material(Shader.Find("Unlit/Texture"));
        }

        // set up transform
        go.transform.Rotate(-90.0f, 0.0f, 0.0f);
        go.transform.position = new Vector3(0.0f,0.0f,3.0f);
        go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        // configure videoSurface
        var videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }

    // Video TYPE 2: RawImage
    private static VideoSurface MakeImageSurface(string goName)
    {
        GameObject go = new GameObject();

        if (go == null)
        {
            return null;
        }

        go.name = goName;
        // to be renderered onto
        go.AddComponent<RawImage>();
        // make the object draggable
        //go.AddComponent<UIElementDrag>();
        var canvas = GameObject.Find("CoachingCardRoot");
        if (canvas != null)
        {
            go.transform.parent = canvas.transform;
            Debug.Log("add video view");
        }
        else
        {
            Debug.Log("Canvas is null video view");
        }

        // set up transform
        go.transform.Rotate(0f, 0.0f, 180.0f);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = new Vector3(2f, 3f, 1f);

        // configure videoSurface
        var videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }

    internal void DestroyVideoView(uint uid)
    {
        var go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            Destroy(go);
        }
    }

    #endregion


    /*public void CreateLocalVideoCallQualityPanel(GameObject parent)
    {
        if (parent.GetComponentInChildren<LocalVideoCallQualityPanel>() != null)
            return;

        var panel = GameObject.Instantiate(this._videoQualityItemPrefab, parent.transform);
        panel.AddComponent<LocalVideoCallQualityPanel>();

    }

    public LocalVideoCallQualityPanel GetLocalVideoCallQualityPanel()
    {
        var go = GameObject.Find("0");
        return go.GetComponentInChildren<LocalVideoCallQualityPanel>();
    }

    public void CreateRemoteVideoCallQualityPanel(GameObject parent, uint uid)
    {
        if (parent.GetComponentInChildren<RemoteVideoCallQualityPanel>() != null)
            return;

        var panel = GameObject.Instantiate(this._videoQualityItemPrefab, parent.transform);
        var comp = panel.AddComponent<RemoteVideoCallQualityPanel>();
        comp.Uid = uid;
    }

    public RemoteVideoCallQualityPanel GetRemoteVideoCallQualityPanel(uint uid)
    {
        var go = GameObject.Find(uid.ToString());
        return go.GetComponentInChildren<RemoteVideoCallQualityPanel>();
    }*/
}

#region -- Agora Event ---

internal class UserEventHandler : IRtcEngineEventHandler
{
    private readonly AgoraRTCManager agoraRtcManager;

    internal UserEventHandler(AgoraRTCManager RtcManager)
    {
        agoraRtcManager = RtcManager;
    }

    public override void OnError(int err, string msg)
    {
        Debug.LogError(msg);
    }

    public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
    {
        // int build = 0;
        // Debug.Log("Agora: OnJoinChannelSuccess ");
        // Debug.Log(string.Format("sdk version: ${0}",
        //     agoraRtcManager.RtcEngine.GetVersion(ref build)));
        // Debug.Log(string.Format("sdk build: ${0}",
        //     build));
        Debug.Log(
            string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                connection.channelId, connection.localUid, elapsed));
    }

    public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
    {
        Debug.Log("OnRejoinChannelSuccess");
    }

    public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
    {
        Debug.Log("OnLeaveChannel");
    }

    public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole,
        CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
    {
        Debug.Log("OnClientRoleChanged");
    }

    public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
    {
        Debug.Log(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        if(agoraRtcManager != null || uid != 0)
            agoraRtcManager.MakeVideoView(uid, agoraRtcManager.GetChannelName());
        else
        {
            Debug.Log(" NULLLL NULLLL");
        }
        //agoraRtcManager.CreateRemoteVideoCallQualityPanel(node, uid);
    }

    public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
    {
        Debug.Log(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
            (int)reason));
        agoraRtcManager.DestroyVideoView(uid);
    }

    //Quality monitoring during calls
    public override void OnRtcStats(RtcConnection connection, RtcStats stats)
    {
        // var panel = agoraRtcManager.GetLocalVideoCallQualityPanel();
        // if (panel != null)
        // {
        //     panel.Stats = stats;
        //     panel.RefreshPanel();
        // }
    }

    public override void OnLocalAudioStats(RtcConnection connection, LocalAudioStats stats)
    {
        // var panel = agoraRtcManager.GetLocalVideoCallQualityPanel();
        // if (panel != null)
        // {
        //     panel.AudioStats = stats;
        //     panel.RefreshPanel();
        // }
    }

    public override void OnLocalAudioStateChanged(RtcConnection connection, LOCAL_AUDIO_STREAM_STATE state,
        LOCAL_AUDIO_STREAM_REASON reason)
    {
    }

    public override void OnRemoteAudioStats(RtcConnection connection, RemoteAudioStats stats)
    {
        // var panel = agoraRtcManager.GetRemoteVideoCallQualityPanel(stats.uid);
        // if (panel != null)
        // {
        //     panel.AudioStats = stats;
        //     panel.RefreshPanel();
        // }
    }

    public override void OnRemoteAudioStateChanged(RtcConnection connection, uint remoteUid, REMOTE_AUDIO_STATE state,
        REMOTE_AUDIO_STATE_REASON reason, int elapsed)
    {
    }

    public override void OnLocalVideoStats(RtcConnection connection, LocalVideoStats stats)
    {
        // var panel = agoraRtcManager.GetLocalVideoCallQualityPanel();
        // if (panel != null)
        // {
        //     panel.VideoStats = stats;
        //     panel.RefreshPanel();
        // }
    }

    public override void OnRemoteVideoStats(RtcConnection connection, RemoteVideoStats stats)
    {
        // var panel = agoraRtcManager.GetRemoteVideoCallQualityPanel(stats.uid);
        // if (panel != null)
        // {
        //     panel.VideoStats = stats;
        //     panel.RefreshPanel();
        // }
    }

    public override void OnRemoteVideoStateChanged(RtcConnection connection, uint remoteUid, REMOTE_VIDEO_STATE state,
        REMOTE_VIDEO_STATE_REASON reason, int elapsed)
    {
    }
}

#endregion