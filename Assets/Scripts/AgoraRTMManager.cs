using System;
using UnityEngine;
using Agora.Rtm;
using System.Threading;
using System.Threading.Tasks;

public class AgoraRTMManager : MonoBehaviour
{
    [SerializeField] private string appId = "";
    [SerializeField] private string username = "";
    [SerializeField] private string userToken = "";
    [SerializeField] private string channelName = "";

    private IRtmClient rtmClient;


    private bool CheckAppId()
    {
        return appId != null && appId != "" && appId.Length > 10;
    }

    private void Start()
    {
        if (CheckAppId())
        {
            Init();
        }
    }

    private async void Init()
    {
        OnInit();
        await OnLoginAsync();
        OnJoin();
    }

    public void OnInit()
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(appId))
        {
            Debug.Log("We need a username and appId to init");
            return;
        }

        RtmConfig config = new RtmConfig();
        config.appId = appId;
        config.userId = username;
        try
        {
            rtmClient = RtmClient.CreateAgoraRtmClient(config);
        }
        catch (RTMException e)
        {
            Debug.Log("rtmClient.init error  ret:" + e.Status.ErrorCode);
        }


        if (rtmClient != null)
        {
            //add observer
            rtmClient.OnMessageEvent += this.OnMessageEvent;
            // rtmClient.OnPresenceEvent += this.OnPresenceEvent;
            // rtmClient.OnTopicEvent += this.OnTopicEvent;
            // rtmClient.OnStorageEvent += this.OnStorageEvent;
            // rtmClient.OnLockEvent += this.OnLockEvent;
            // rtmClient.OnConnectionStateChanged += this.OnConnectionStateChanged;
            // rtmClient.OnTokenPrivilegeWillExpire += this.OnTokenPrivilegeWillExpire;


            //var ret = rtmClient.SetParameters("{\"rtm.link_address0\":[\"183.131.160.141\", 9130]}");
            //RtmScene.AddMessage("rtmClient.SetParameters + ret:" + ret, Message.MessageType.Info);
            //ret = rtmClient.SetParameters("{\"rtm.link_address1\":[\"183.131.160.142\", 9131]}");
            //RtmScene.AddMessage("rtmClient.SetParameters + ret:" + ret, Message.MessageType.Info);
            //ret = rtmClient.SetParameters("{\"rtm.link_encryption\": false}");
            //RtmScene.AddMessage("rtmClient.SetParameters + ret:" + ret, Message.MessageType.Info);
            ////ret = rtmClient.SetParameters("{\"rtm.ap_address\":[\"114.236.137.40\", 8443]}");
            //RtmScene.AddMessage("rtmClient.SetParameters + ret:" + ret, Message.MessageType.Info);


            Debug.Log("RtmClient init success");
        }
    }


    public async void OnJoin()
    {
        if (rtmClient == null)
        {
            Debug.LogError("rtmClient is null");
            return;
        }

        SubscribeOptions options = new SubscribeOptions()
        {
            withMessage = true
        };
    
        var result2 = await rtmClient.SubscribeAsync(channelName, options);
        var status2 = result2.Status;
    
        if (status2.Error)
        {
            Debug.LogError($"Subscribe failed: {status2.ErrorCode} - {status2.Reason}");
        }
        else
        {
            Debug.Log($"Successfully subscribed to channel: {channelName}");
        }
    }

    public async void OnLeave()
    {
        if (rtmClient != null)
        {
            var result = await rtmClient.UnsubscribeAsync(channelName);
            var status2 = result.Status;
            var response2 = result.Response;
            if (status2.Error)
            {
                Debug.Log(string.Format("{0} is failed", status2.Operation));
                Debug.Log(string.Format("The error code is {0}, because of: {1}", status2.ErrorCode, status2.Reason));
            }
            else
            {
                Debug.Log("Unsubscribe Channel Success!");
            }

            var status3 = rtmClient.Dispose();
            Debug.Log("Dispose rtmClient Success!");
            rtmClient = null;
        }
    }

    private void OnMessageEvent(MessageEvent eve)
    {
        var channelName = eve.channelName;
        var channelType = eve.channelType;
        var topic = eve.channelTopic;
        var publisher = eve.publisher;
        var messageType = eve.messageType;
        var customType = eve.customType;
        var message = eve.message;
        if (messageType == RTM_MESSAGE_TYPE.STRING)
        {
            var stMessage = message.GetData<string>();
            Debug.Log(string.Format("You have recieved a string type message: {0} from: {1} in channel:{2}", stMessage,
                publisher, channelName));
            Debug.Log(string.Format("The channel type is {0}", channelType));
        }
        else
        {
            var biMessage = message.GetData<byte[]>();
            Debug.Log(string.Format("You have recieved a binary type message: {0},from: {1} in channel:{2}",
                System.BitConverter.ToString(biMessage), publisher, channelName));
            Debug.Log(string.Format("The channel type is {0}", channelType));
        }
    }


    public async Task OnLoginAsync()
    {
        if (rtmClient == null)
        {
            Debug.Log("RtmClient not init!!!");
            return;
        }

        var result = await rtmClient.LoginAsync(userToken);

        if (result.Status.Error)
        {
            Debug.Log("rtmClient.Login + ret:" + result.Status.ErrorCode);
        }
        else
        {
            Debug.Log("rtmClient.Login + respones:");
        }
    }

    public async void OnLogoutAsync()
    {
        if (rtmClient == null)
        {
            Debug.Log("RtmClient not init!!!");
            return;
        }

        var ret = await rtmClient.LogoutAsync();
        Debug.Log(string.Format("RtmClient.Logout ret:{0} ", ret.Status.ErrorCode));
    }

    private void OnDestroy()
    {
        OnLeave();
        OnLogoutAsync();
    }
}