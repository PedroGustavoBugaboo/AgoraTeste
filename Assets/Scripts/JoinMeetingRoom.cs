using Agora.Rtc;
using UnityEngine;
using UnityEngine.Serialization;

public class JoinMeetingRoom : MonoBehaviour
{
    [FormerlySerializedAs("RoomSetup")] [SerializeField]
    private AgoraRoomSetup _agoraRoomSetup;


    [Header("_____________Basic Configuration_____________")] [FormerlySerializedAs("APP_ID")] [SerializeField]
    private string _appID = "";

    [FormerlySerializedAs("TOKEN")] [SerializeField]
    private string _token = "";

    [FormerlySerializedAs("CHANNEL_NAME")] [SerializeField]
    private string _channelName = "";

    internal Logger Log;
    internal IRtcEngineEx RtcEngine = null;

    public uint Uid1 = 123;
    public uint Uid2 = 456;

    private void Start()
    {
        LoadAssetData();
        if (_appID.Length > 10)
        {
            InitEngine();

            JoinChannel();
        }
    }

    [ContextMenu("ShowAgoraBasicProfileData")]
    private void LoadAssetData()
    {
        if (_agoraRoomSetup == null) return;
        _appID = _agoraRoomSetup.appID;
        _token = _agoraRoomSetup.token;
        _channelName = _agoraRoomSetup.channelName;
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
        RtcEngine.JoinChannel(_token, _channelName, this.Uid1, options);
    }


    private void InitEngine()
    {
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();
        // UserEventHandler handler = new UserEventHandler(this);
        RtcEngineContext context = new RtcEngineContext();
        context.appId = _appID;
        context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
        context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
        context.areaCode = AREA_CODE.AREA_CODE_GLOB;
        RtcEngine.Initialize(context);
        // RtcEngine.InitEventHandler(handler);
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestroy");
        if (RtcEngine == null) return;
        RtcEngine.InitEventHandler(null);
        RtcEngine.LeaveChannel();
        RtcEngine.Dispose();
    }

    internal string GetChannelName()
    {
        return _channelName;
    }
}