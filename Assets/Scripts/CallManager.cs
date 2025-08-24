using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

[Serializable]
public class TokenResponse
{
    public string channelName;
    public int uid;
    public string account;
    public string token;
}

public class CallManager : MonoBehaviour
{
    private AgoraRTCManager agoraRTCManager;
    private AgoraRTMManager agoraRTMManager;
    private uint uid;
    private string channelToken;
    private string rtmToken;
    [SerializeField] private AudioClip JoinCallClip;
    [SerializeField] private AudioClip LeaveCallClip;

    
    [SerializeField] private AppVariables appVariables;

    [SerializeField] private string serverBaseUrl = "http://192.168.1.7:5000"; 

    private bool CheckAppId()
    {
        string appId = appVariables.appID;
        return !string.IsNullOrEmpty(appId) && appId.Length > 10;
    }

    private void InitializeEngines()
    {
        if (!CheckAppId())
            return;

        agoraRTCManager = new AgoraRTCManager(appVariables.appID, channelToken, appVariables.channelName);
        agoraRTMManager = new AgoraRTMManager(appVariables.appID, appVariables.username, rtmToken, appVariables.channelName);

        agoraRTCManager.InitEngine();
        agoraRTMManager.OnInit();
    }

    public void JoinCall()
    {
        StartCoroutine(FetchTokensAndJoin());
        AudioManager.instance.PlayOneShot(JoinCallClip);
    }

    private IEnumerator FetchTokensAndJoin()
    {
        // RTC Token
        string rtcUrl = $"{serverBaseUrl}/token/uid?channelName={appVariables.channelName}&uid={uid}";
        UnityWebRequest rtcRequest = UnityWebRequest.Get(rtcUrl);
        yield return rtcRequest.SendWebRequest();

        if (rtcRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Errore richiesta RTC token: {rtcRequest.error}");
            yield break;
        }

        TokenResponse rtcData = JsonUtility.FromJson<TokenResponse>(rtcRequest.downloadHandler.text);
        channelToken = rtcData.token;

        // RTM Token
        string rtmUrl = $"{serverBaseUrl}/token/rtm?channelName={appVariables.channelName}&account={appVariables.username}";
        UnityWebRequest rtmRequest = UnityWebRequest.Get(rtmUrl);
        yield return rtmRequest.SendWebRequest();

        if (rtmRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Errore richiesta RTM token: {rtmRequest.error}");
            yield break;
        }

        TokenResponse rtmData = JsonUtility.FromJson<TokenResponse>(rtmRequest.downloadHandler.text);
        rtmToken = rtmData.token;

        Debug.Log("Token RTC e RTM ricevuti correttamente");

        InitializeEngines();
        agoraRTCManager.JoinChannel(uid);
        yield return new WaitForSeconds(1f);
        agoraRTMManager.JoinChannel(uid.ToString());
    }

    private void Start()
    {
        uid = (uint) Random.Range(0, int.MaxValue);
    }

    public void LeaveCall()
    {
        if (agoraRTCManager != null)
        {
            agoraRTCManager.OnApplicationQuit();
            agoraRTCManager = null;
        }
        if (agoraRTMManager != null)
        {
            agoraRTMManager.OnDestroy(uid.ToString());
            agoraRTMManager = null;
        }
        UserManager.instance.OnCallQuit();
        AnchorManager.instance.OnCallQuit();
        AudioManager.instance.PlayOneShot(LeaveCallClip);

    }
    
    private void OnApplicationQuit()
    {
        LeaveCall();
    }

    private void OnDestroy()
    {
        if (agoraRTCManager != null)
        {
            agoraRTCManager.OnApplicationQuit();
            agoraRTMManager = null;
        }

        if (agoraRTMManager != null)
        {
            agoraRTMManager.OnDestroy(uid.ToString());
            agoraRTMManager = null;
        }
    }
}
