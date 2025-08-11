using System;
using UnityEngine;
using Agora.Rtm;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Serialization;

public class AgoraRTMManager
{
    private string appId = "";
    private string username = "";
    private string userToken = "";
    private string channelName = "";
    // private AppVariables appVariables;
    private IRtmClient rtmClient;


    public AgoraRTMManager(string appId, string username, string userToken, string channelName)
    {
        this.appId = appId;
        this.username = username;
        this.userToken = userToken;
        this.channelName = channelName;
    }

    // private bool CheckAppId()
    // {
    //     return appId != null && appId != "" && appId.Length > 10;
    // }

    // private void Start()
    // {
    //     appId = appVariables.appID;
    //     username = appVariables.username;
    //     userToken = appVariables.rtmToken;
    //     channelName = appVariables.channelName;
    //     if (CheckAppId())
    //     {
    //         Init();
    //     }
    // }

    // private async void Init()
    // {
    //     OnInit();
    //     await OnLoginAsync();
    //     OnJoin();
    // }

    public async void JoinChannel(string uid)
    {
        await OnLoginAsync();
        OnJoin(uid);
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
            rtmClient.OnStorageEvent += this.OnStorageEvent;
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

    public async void OnJoin(string uid)
    {
        if (rtmClient == null)
        {
            Debug.LogError("rtmClient is null");
            return;
        }

        SubscribeOptions options = new SubscribeOptions()
        {
            withMessage = true,
            withPresence = true,
            withMetadata = true,
            withLock = true
        };
    
        var result2 = await rtmClient.SubscribeAsync(channelName, options);
        var status2 = result2.Status;
    
        
        var metadata = new RtmMetadata();
        metadata.majorRevision = -1;
        var apple = new MetadataItem()
        {
            key = uid,
            value = username,
            revision = -1
        };
     
        metadata.metadataItems = new MetadataItem[] { apple };
        metadata.metadataItemsSize = 1;
        var metadataOptions = new MetadataOptions()
        {
            recordUserId = true,
            recordTs = true
        };
        var lockName = "";

        var status = await rtmClient.GetStorage().SetChannelMetadataAsync(channelName, RTM_CHANNEL_TYPE.MESSAGE, metadata, metadataOptions, lockName);
        
        FetchChannelMetadata();
        if (status.Status.Error)
        {
            Debug.Log(string.Format("{0} is failed, ErrorCode: , due to: ", status.Status.ErrorCode));
        }
        else
        {
            Debug.Log(string.Format("Set Channel :{0} metadata success! Channel Type is :{1}! ", status.Response.ChannelName, status.Response.ChannelType));
        }

        if (status2.Error)
        {
            Debug.LogError($"Subscribe failed: {status2.ErrorCode} - {status2.Reason}");
        }
        else
        {
            Debug.Log($"Successfully subscribed to channel: {channelName}");
        }
    }

    public async void OnLeave(string uid)
    {
        if (rtmClient != null)
        {
            var metadata = new RtmMetadata();
            metadata.majorRevision = -1;
            var metadataItem = new MetadataItem()
            {
                key = uid,
                revision = -1
            };
            metadata.metadataItems = new MetadataItem[] { metadataItem };
            metadata.metadataItemsSize = 1;
            var options = new MetadataOptions() { };

            var result1 = await rtmClient.GetStorage().RemoveChannelMetadataAsync(channelName, RTM_CHANNEL_TYPE.MESSAGE, metadata,options, "");
            if (result1.Status.Error)
            {
                Debug.Log(string.Format("{0} is failed, ErrorCode: {1}, due to: {2}", result1.Status.Operation, result1.Status.ErrorCode, result1.Status.Reason));
            }
            else
            {
                Debug.Log(string.Format("Remove Channel :{0} metadata success! Channel Type is :{1}! ", result1.Response.ChannelName, result1.Response.ChannelType));
            }
            
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
    
    private void OnStorageEvent(StorageEvent @event)
    {
        Debug.Log($"[RTM][StorageEvent] type={@event.eventType} channelType={@event.channelType}");
        FetchChannelMetadata();
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
            
            ChatManager.instance.CreateMessage(publisher, stMessage);
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

    public void FetchChannelMetadata()
    {
        if (rtmClient == null)
        {
            Debug.LogWarning("FetchChannelMetadata called but rtmClient is null");
            return;
        }
        _ = FetchChannelMetadataInternal(); // esegue in background
    }

    private async Task FetchChannelMetadataInternal()
    {
        try
        {
            var result = await rtmClient.GetStorage()
                .GetChannelMetadataAsync(channelName, RTM_CHANNEL_TYPE.MESSAGE);

            if (result.Status.Error)
            {
                Debug.LogError($"GetChannelMetadata failed: {result.Status.ErrorCode} - {result.Status.Reason}");
                return;
            }

            var meta = result.Response.Data;
            if (meta == null || meta.metadataItems == null)
            {
                Debug.Log("[RTM] Channel metadata vuoto");
                return;
            }

            foreach (var user in UserManager.instance.Users)
            {
                if(user.RtcID == null || user.RtmID != null)
                    continue;
                
                foreach (var item in meta.metadataItems)
                {
                    if (item.key.Equals(user.RtcID))
                    {
                        user.RtmID = item.value;
                    }
                }
            }
            

        }
        catch (Exception ex)
        {
            Debug.LogError("Eccezione in FetchChannelMetadataInternal: " + ex.Message);
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


    public void OnDestroy(string uid)
    {
        OnLeave(uid);
    }
}