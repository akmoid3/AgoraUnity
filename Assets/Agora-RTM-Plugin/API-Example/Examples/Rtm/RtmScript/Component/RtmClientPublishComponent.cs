﻿
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if AGORA_RTC
using Agora.Rtc;
#endif
using Agora.Rtm;

namespace io.agora.rtm.demo
{
    public class RtmClientPublishComponent : IRtmComponet
    {
        public InputField ChannelNameInput;
        public InputField MessageInput;

        public EnumDropDown EnumDropDown;
        public InputField CustomTypeInput;
    
        public void Start()
        {
            this.EnumDropDown.Init<RTM_CHANNEL_TYPE>();
        }

        public async void OnPublish()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient not init!!!", Message.MessageType.Error);
                return;
            }

            string message = MessageInput.text;
            if (message == "")
            {
                RtmScene.AddMessage("Message is empty", Message.MessageType.Error);
                return;
            }

            string channelName = this.ChannelNameInput.text;

            PublishOptions options = new PublishOptions();
            options.channelType = (RTM_CHANNEL_TYPE)this.EnumDropDown.GetSelectValue();
            options.customType = this.CustomTypeInput.text;

            var result = await RtmScene.RtmClient.PublishAsync(channelName, message, options);

            if (result.Status.Error)
            {
                RtmScene.AddMessage(string.Format("rtmClient.Publish Status.ErrorCode:{0} ", result.Status.ErrorCode), Message.MessageType.Error);
            }
            else
            {
                string info = string.Format("rtmClient.Publish Response");
                RtmScene.AddMessage(info, Message.MessageType.Info);
            }

        }
    }
}
