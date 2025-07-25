﻿
#define AGORA_RTM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtm;
namespace io.agora.rtm.demo
{
    public class RtmClientLoginComponent : IRtmComponet
    {
        public Text TitleText;
        public InputField TokenInput;

        public void Start()
        {
            this.TokenInput.text = RtmScene.InfoInput.token;
            this.TitleText.text = "RtmClient not login";
            this.TitleText.color = Color.red;
        }

        public async void OnLoginAsync()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient not init!!!", Message.MessageType.Error);
                return;
            }

            string token = this.TokenInput.text;
            var result = await RtmScene.RtmClient.LoginAsync(token);

            if (result.Status.Error)
            {
                RtmScene.AddMessage("rtmClient.Login + ret:" + result.Status.ErrorCode, Message.MessageType.Error);
                this.TitleText.text = "RtmClient login failed";
                this.TitleText.color = Color.red;
            }
            else
            {
                RtmScene.AddMessage("rtmClient.Login + respones:", Message.MessageType.Info);
                this.TitleText.text = "RtmClient already login";
                this.TitleText.color = Color.green;
            }

        }

        public async void OnLogoutAsync()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient not init!!!", Message.MessageType.Error);
                return;
            }

            if (RtmScene.StreamChannel != null)
            {
                RtmScene.StreamChannel.Dispose();
                RtmScene.StreamChannel = null;
                RtmScene.AddMessage("StreamChannel Disposed", Message.MessageType.Info);
            }


            var ret = await RtmScene.RtmClient.LogoutAsync();
            RtmScene.AddMessage(string.Format("RtmClient.Logout ret:{0} ", ret.Status.ErrorCode), Message.MessageType.Info);
            this.TitleText.text = "RtmClient logout";
            this.TitleText.color = Color.red;
        }

        public async void OnRenewTokenAsync()
        {
            if (RtmScene.RtmClient == null)
            {
                RtmScene.AddMessage("RtmClient not init!!!", Message.MessageType.Error);
                return;
            }

            string token = this.TokenInput.text;
            var res =  await RtmScene.RtmClient.RenewTokenAsync(token);
            RtmScene.AddMessage("rtmClient RenewToken: " + res.Status.ErrorCode, Message.MessageType.Info);

        }


    }
}
