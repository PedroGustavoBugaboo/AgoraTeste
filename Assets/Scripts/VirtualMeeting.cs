using System;
using System.Collections.Generic;
using System.Linq;
using Agora.Rtc;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class VirtualMeeting : MonoBehaviour
{
    [FormerlySerializedAs("RoomSetup")] [SerializeField]
    private AgoraRoomSetup _agoraRoomSetup;

    [SerializeField] private GameObject ShareBtnObject;

    [Header("_____________Basic Configuration_____________")] [FormerlySerializedAs("APP_ID")] [SerializeField]
    private string _appID = "d1e8f062b68e4add946192c0a7aa04c3";

    [FormerlySerializedAs("TOKEN")] [SerializeField]
    private string _token = "";

    [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
    private string _channelName = "";
    
    private RenderTexture renderTexture;

    internal Logger Log;
    internal IRtcEngineEx RtcEngine = null;

    public uint Uid1 = 123;
    public uint Uid2 = 456;

    private TMP_Dropdown _winIdSelect;
    private Button _startBtn;

    private bool sharing;

    private void Start()
    {
        LoadAssetData();
        if (CheckAppId())
        {
            InitEngine();
#if UNITY_ANDROID || UNITY_IPHONE
#else
            PrepareScreenCapture();
#endif
            JoinChannel();
        }
    }

    #region PrepareAmbient

    [ContextMenu("ShowAgoraBasicProfileData")]
    private void LoadAssetData()
    {
        if (_agoraRoomSetup == null) return;
        _appID = _agoraRoomSetup.appID;
        _token = _agoraRoomSetup.token;
        _channelName = _agoraRoomSetup.channelName;
    }

    private bool CheckAppId()
    {
        return !string.IsNullOrEmpty(_appID) && _appID.Length > 10;
    }

    private void InitEngine()
    {
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();
        if (RtcEngine == null)
        {
            Debug.LogError("Falha ao criar o RtcEngine.");
            return;
        }

        UserEventHandler handler = new UserEventHandler(this);
        RtcEngineContext context = new RtcEngineContext
        {
            appId = _appID,
            channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT,
            areaCode = AREA_CODE.AREA_CODE_GLOB
        };

        int result = RtcEngine.Initialize(context);
        if (result != 0)
        {
            Debug.LogError($"Falha ao inicializar o RtcEngine. Código de erro: {result}");
            return;
        }

        RtcEngine.InitEventHandler(handler);
        Debug.Log("RtcEngine inicializado com sucesso.");
    }

    private void PrepareScreenCapture()
    {
        _winIdSelect = GameObject.Find("winIdSelect").GetComponent<TMP_Dropdown>();

        if (_winIdSelect == null || RtcEngine == null) return;

        _winIdSelect.ClearOptions();

        SIZE size = new SIZE { width = 360, height = 240 };
        var info = RtcEngine.GetScreenCaptureSources(size, size, true);

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        foreach (var screen in info)
        {
            var option = new TMP_Dropdown.OptionData
            {
                image = null,
                text = string.Format("{0}: {1}-{2} | {3}", screen.type, screen.sourceName, screen.sourceTitle,
                    screen.sourceId)
            };

            options.Add(option);
        }

        _winIdSelect.AddOptions(options);
    }

    private void JoinChannel()
    {
        RtcEngine.EnableAudio();
        RtcEngine.EnableVideo();
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        ChannelMediaOptions options = new ChannelMediaOptions();
        options.autoSubscribeAudio.SetValue(true);
        options.autoSubscribeVideo.SetValue(true);

        options.publishCameraTrack.SetValue(true);
        options.publishScreenTrack.SetValue(false);
        options.enableAudioRecordingOrPlayout.SetValue(true);
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        RtcEngine.JoinChannel(_token, _channelName, Uid1, options);
    }

    #endregion

    #region StartTransmition

    public void ClickButton()
    {
        if (ShareBtnObject.GetComponent<Toggle>().isOn) StartScreenShare();
        else OnStopShareBtnClick();
    }

    private void StartScreenShare()
    {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        StartScreenSharePC();
#elif UNITY_ANDROID || UNITY_IOS
        StartScreenShareMobile();
#endif
        ScreenShareJoinChannel();

        UpdateChannelMediaOptions();
        
        var obj = ShareBtnObject;
        var txt = obj.GetComponentInChildren<TextMeshProUGUI>();
        txt.text = "Stop Share";
    }

    private void StartScreenSharePC()
    {
        RtcEngine.StopScreenCapture();

        var option = _winIdSelect.options[_winIdSelect.value].text;
        Debug.Log("Selected screen/window option for screen share: " + option);

        int ret;

        if (option.Contains("ScreenCaptureSourceType_Window"))
        {
            var windowId = option.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            Debug.Log("Starting screen capture by window ID: " + windowId);

            ret = RtcEngine.StartScreenCaptureByWindowId(ulong.Parse(windowId), default(Rectangle),
                new ScreenCaptureParameters { captureMouseCursor = true, frameRate = 30 });
        }
        else
        {
            var dispId = uint.Parse(option.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1]);
            Debug.Log("Starting screen capture by display ID: " + dispId);

            ret = RtcEngine.StartScreenCaptureByDisplayId(dispId, default(Rectangle),
                new ScreenCaptureParameters { captureMouseCursor = true, frameRate = 30 });
        }

        if (ret != 0)
        {
            Debug.LogError("Failed to start screen capture, error code: " + ret);
        }
        else
        {
            Debug.Log("Screen capture started successfully.");
        }
    }

    private void StartScreenShareMobile()
    {
        var parameters2 = new ScreenCaptureParameters2();
        parameters2.captureAudio = true;
        parameters2.captureVideo = true;
        RtcEngine.StartScreenCapture(parameters2);
    }

    private void ScreenShareJoinChannel()
    {
        ChannelMediaOptions options = new ChannelMediaOptions();
        options.autoSubscribeAudio.SetValue(true);
        options.autoSubscribeVideo.SetValue(false);
        options.publishCameraTrack.SetValue(false);
        options.publishScreenTrack.SetValue(true);
        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);


#if UNITY_ANDROID || UNITY_IPHONE
            options.publishScreenCaptureAudio.SetValue(true);
            options.publishScreenCaptureVideo.SetValue(true);
#endif
        
        var ret = RtcEngine.JoinChannelEx(_token, new RtcConnection(_channelName, this.Uid2), options);
        if (ret != 0)
        {
            Debug.LogError("Failed to join channel, error code: " + ret);
        }
        else
        {
            Debug.Log("JoinChannelEx returns: " + ret);
        }
    }

    #endregion

    #region StopTransmission

    private void OnStopShareBtnClick()
    {
        ScreenShareLeaveChannel();
        var txt = ShareBtnObject.GetComponentInChildren<TextMeshProUGUI>();
        txt.text = "Start Sharing";
        RtcEngine.StopScreenCapture();
    }

    private void ScreenShareLeaveChannel()
    {
        RtcEngine.LeaveChannelEx(new RtcConnection(_channelName, Uid2));
    }

    #endregion

    private void UpdateChannelMediaOptions()
    {
        ChannelMediaOptions options = new ChannelMediaOptions();
        options.autoSubscribeAudio.SetValue(false);
        options.autoSubscribeVideo.SetValue(false);

        options.publishCameraTrack.SetValue(false);
        options.publishScreenTrack.SetValue(true);

#if UNITY_ANDROID || UNITY_IPHONE
            options.publishScreenCaptureAudio.SetValue(true);
            options.publishScreenCaptureVideo.SetValue(true);
#endif

        options.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        var ret = RtcEngine.UpdateChannelMediaOptions(options);
        Debug.Log("UpdateChannelMediaOptions returns: " + ret);
    }

    private void OnDestroy()
    {
        if (RtcEngine == null) return;
        RtcEngine.InitEventHandler(null);
        RtcEngine.LeaveChannel();
        RtcEngine.Dispose();
    }

    internal string GetChannelName()
    {
        return _channelName;
    }

    #region -- Video Render UI Logic ---

    internal static void MakeVideoView(uint uid, string channelId = "",
        VIDEO_SOURCE_TYPE videoSourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
    {
        var go = GameObject.Find(uid.ToString());
        if (go != null)
        {
            return;
        }
        
        var canvas = GameObject.Find("VideoCanvas");
        
        var goObject = new GameObject { name = uid.ToString() };
        goObject.transform.SetParent(canvas.transform);
        goObject.transform.localPosition = Vector3.zero;
        goObject.transform.localScale = new Vector3(1f, 1f, 1f);

        var rawImage = goObject.AddComponent<RawImage>();
        var videoSurface = goObject.AddComponent<VideoSurface>();
        
        videoSurface.SetForUser(uid, channelId, videoSourceType);
        videoSurface.SetEnable(true);
        
        rawImage.texture = videoSurface.GetComponent<RawImage>()?.texture;
        
        videoSurface.OnTextureSizeModify += (int width, int height) =>
        {
            float scale = (float)height / (float)width;
            goObject.transform.localScale = new Vector3(-5, 5 * scale, 1);
            Debug.Log($"OnTextureSizeModify: {width} x {height}");
        };
    }

    internal static void DestroyVideoView(string name)
    {
        var go = GameObject.Find(name);
        if (go != null)
        {
            Destroy(go);
        }
    }

    #endregion

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly VirtualMeeting _desktopScreenShare;

        internal UserEventHandler(VirtualMeeting desktopScreenShare)
        {
            _desktopScreenShare = desktopScreenShare;
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            if (connection.localUid == _desktopScreenShare.Uid1)
            {
                MakeVideoView(0);
            }
            else if (connection.localUid == _desktopScreenShare.Uid2)
            {
                MakeVideoView(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN);
            }
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            if (connection.localUid == _desktopScreenShare.Uid1)
            {
                DestroyVideoView("MainCameraView");
            }
            else if (connection.localUid == _desktopScreenShare.Uid2)
            {
                DestroyVideoView("ScreenShareView");
            }
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            if (uid != _desktopScreenShare.Uid1 && uid != _desktopScreenShare.Uid2)
            {
                MakeVideoView(uid, _desktopScreenShare.GetChannelName(), VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            }
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            if (uid != _desktopScreenShare.Uid1 && uid != _desktopScreenShare.Uid2)
            {
                DestroyVideoView(uid.ToString());
            }
        }
    }
}