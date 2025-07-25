
#define AGORA_RTM
#define AGORA_FULL


using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if AGORA_RTC
using Agora.Rtc;
using io.agora.rtc.demo;
#else
using io.agora.rtm.demo;
#endif

using System;

namespace Agora_RTC_Plugin.API_Example
{
    public class RtmHome : MonoBehaviour
    {
        public InputField AppIdInupt;
        public InputField ChannelInput;
        public InputField TokenInput;

        public AppIdInput AppInputConfig;
        public GameObject CasePanel;
        public GameObject CaseScrollerView;

        public GameObject EventSystem;

        private string _playSceneName = "";



        private string[] _rtcNameList = {
#if AGORA_RTC
//#if AGORA_NUMBER_UID
            "BasicAudioCallScene",
            "BasicVideoCallScene",
            "AudioMixingScene",
            "AudioSpectrumScene",
            "ChannelMediaRelayScene",
            "ContentInspectScene",
            "CustomCaptureAudioScene",
            "CustomCaptureVideoScene",
            "CustomRenderAudioScene",
            "DeviceManagerScene",
            "DualCameraScene",
            "JoinChannelVideoTokenScene",
            "JoinChannelWithUserAccountScene",
            "MediaPlayerScene",
            "MediaPlayerWithCustomDataProviderScene",
            "MediaRecorderScene",
            "MetadataScene",
            "MusicPlayerScene",
            "PluginScene",
            "ProcessAudioRawDataScene",
            "ProcessVideoRawDataScene",
            "PushEncodedVideoImageScene",
            "RenderWithYUV",
            "ScreenShareScene",
            "ScreenShareWhileVideoCallScene",
            "SetBeautyEffectOptionsScene",
            "SetEncryptionScene",
            "SetVideoEncodeConfigurationScene",
            "SpatialAudioWithMediaPlayerScene",
            "SpatialAudioWithUsers",
            "StartDirectCdnStreamingScene",
            "StartLocalVideoTranscoderScene",
            "StartRhythmPlayerScene",
            "StartRtmpStreamWithTranscodingScene",
            "StreamMessageScene",
            "TakeSnapshotScene",
            "VirtualBackgroundScene",
            "VoiceChangerScene",
            "WriteBackVideoRawDataScene",
//#endif
//#if AGORA_STRING_UID
//            "BasicAudioCallSceneS",
//            "BasicVideoCallSceneS",
//            "AudioMixingSceneS",
//            "AudioSpectrumSceneS",
//            "ChannelMediaRelaySceneS",
//            "ContentInspectSceneS",
//            "CustomCaptureAudioSceneS",
//            "CustomCaptureVideoSceneS",
//            "CustomRenderAudioSceneS",
//            "DeviceManagerSceneS",
//            "DualCameraSceneS",
//            "JoinChannelVideoTokenSceneS",
//            "MediaPlayerSceneS",
//            "MediaPlayerWithCustomDataProviderSceneS",
//            "MediaRecorderSceneS",
//            "MetadataSceneS",
//            "MusicPlayerSceneS",
//            "PluginSceneS",
//            "ProcessAudioRawDataSceneS",
//            "ProcessVideoRawDataSceneS",
//            "PushEncodedVideoImageSceneS",
//            "RenderWithYUVS",
//            "ScreenShareSceneS",
//            "ScreenShareWhileVideoCallSceneS",
//            "SetBeautyEffectOptionsSceneS",
//            "SetEncryptionSceneS",
//            "SetVideoEncodeConfigurationSceneS",
//            "SpatialAudioWithMediaPlayerSceneS",
//            "SpatialAudioWithUsersS",
//            "StartDirectCdnStreamingSceneS",
//            "StartLocalVideoTranscoderSceneS",
//            "StartRhythmPlayerSceneS",
//            "StartRtmpStreamWithTranscodingSceneS",
//            "StreamMessageSceneS",
//            "TakeSnapshotSceneS",
//            "VirtualBackgroundSceneS",
//            "VoiceChangerSceneS",
//            "WriteBackVideoRawDataSceneS",
//#endif
#endif
        };

        private string[] _rtmNameList = {
#if AGORA_RTM
            "RtmClientScene",
            "RtmStreamChannelScene",
            "RtmLockScene",
            "RtmPresenceScene",
            "RtmStorageScene"
#endif
        };

        private void Awake()
        {
#if AGORA_RTC
            PermissionHelper.RequestMicrophontPermission();
            PermissionHelper.RequestCameraPermission();
#endif

            GameObject content = GameObject.Find("Content");
            var contentRectTrans = content.GetComponent<RectTransform>();

            for (int i = 0; i < _rtcNameList.Length; i++)
            {
                var go = Instantiate(CasePanel, content.transform);
                var name = go.transform.Find("Text").gameObject.GetComponent<Text>();
                name.text = _rtcNameList[i];
                var button = go.transform.Find("Button").gameObject.GetComponent<Button>();
                button.onClick.AddListener(OnJoinSceneClicked);
                button.onClick.AddListener(SetScolllerActive);
            }

            for (int i = 0; i < _rtmNameList.Length; i++)
            {
                var go = Instantiate(CasePanel, content.transform);
                var name = go.transform.Find("Text").gameObject.GetComponent<Text>();
                name.text = _rtmNameList[i];
                var button = go.transform.Find("Button").gameObject.GetComponent<Button>();
                button.onClick.AddListener(OnJoinSceneClicked);
                button.onClick.AddListener(SetScolllerActive);
            }


            if (this.AppInputConfig)
            {
                this.AppIdInupt.text = this.AppInputConfig.appID;
                this.ChannelInput.text = this.AppInputConfig.channelName;
                this.TokenInput.text = this.AppInputConfig.token;
            }

        }

        // Start is called before the first frame update
        private void Start()
        {


        }

        // Update is called once per frame
        private void Update()
        {

        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
        }

        public void OnLeaveButtonClicked()
        {
            StartCoroutine(UnloadSceneAsync());
            CaseScrollerView.SetActive(true);
        }

        public IEnumerator UnloadSceneAsync()
        {
            if (this._playSceneName != "")
            {
                AsyncOperation async = SceneManager.UnloadSceneAsync(_playSceneName);
                yield return async;
                EventSystem.gameObject.SetActive(true);
            }
        }

        public void OnJoinSceneClicked()
        {
            this.AppInputConfig.appID = this.AppIdInupt.text;
            this.AppInputConfig.channelName = this.ChannelInput.text;
            this.AppInputConfig.token = this.TokenInput.text;

            var button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            var sceneName = button.transform.parent.Find("Text").gameObject.GetComponent<Text>().text;

            EventSystem.gameObject.SetActive(false);

            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            this._playSceneName = sceneName;

        }

        public void SetScolllerActive()
        {
            CaseScrollerView.SetActive(false);
        }
    }
}
