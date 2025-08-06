using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CallManager : MonoBehaviour
{
    private AgoraRTCManager agoraRTCManager;
    private AgoraRTMManager agoraRTMManager;
    private uint uid;
    
    [SerializeField] private AppVariables appVariables;

    
    private bool CheckAppId()
    {
        string appId = appVariables.appID;
        return appId != null && appId != "" && appId.Length > 10;
    }

    private void InitializeEngines()
    {
        if(!CheckAppId())
            return;
        
        agoraRTCManager = new AgoraRTCManager(appVariables.appID,appVariables.tokenChannel,appVariables.channelName);
        agoraRTMManager = new AgoraRTMManager(appVariables.appID,appVariables.username,appVariables.rtmToken, appVariables.channelName);
        
        agoraRTCManager.InitEngine();
        agoraRTMManager.OnInit();
    }

    public void JoinCall()
    {
        agoraRTCManager.JoinChannel(uid);
        agoraRTMManager.JoinChannel(uid.ToString());
    }

    private void Start()
    {
        InitializeEngines();
        uid = (uint) UnityEngine.Random.Range(0, int.MaxValue);

    }

    private void OnApplicationQuit()
    {
        agoraRTCManager.OnApplicationQuit();
        agoraRTMManager.OnDestroy();
    }

    private void Update()
    {
        // PermissionHelper.RequestMicrophontPermission();
        // //PermissionHelper.RequestCameraPermission();
    }
    
    
}
