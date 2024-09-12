using Agora.Rtc;
using UnityEngine;
using UnityEngine.UI;

namespace Agora_RTC_Plugin.API_Example.Examples.Advanced.ScreenShare.Client
{
    public class ScreenShareClient : MonoBehaviour
    {
        private string _appID = "";
        private string _token = "";
        private string _channelName = "";

        public Text LogText;
        internal Logger Log;
        internal IRtcEngine RtcEngine = null;

        private uint trackId;

        [SerializeField] private DataLogin _dataLogin;

        // Use this for initialization
        private void Start()
        {
            LoadAssetData();
            if (CheckAppId())
            {
                InitEngine();
                SetBasicConfiguration();
                JoinChannel();   
            }
        }

        private bool CheckAppId()
        {
            Log = new Logger(LogText);
            return Log.DebugAssert(_appID.Length > 10, "Please fill in your appId in API-Example/profile/appIdInput.asset");
        }

        //Show data in AgoraBasicProfile
        [ContextMenu("ShowAgoraBasicProfileData")]
        private void LoadAssetData()
        {
            _appID = _dataLogin._appID;
            _token = _dataLogin._token;
            _channelName = _dataLogin._channelName;
        }

        private void InitEngine()
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
            UserEventHandler handler = new UserEventHandler(this);
            RtcEngineContext context = new RtcEngineContext();
            context.appId = _appID;
            context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
            context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
            context.areaCode = AREA_CODE.AREA_CODE_GLOB;
            RtcEngine.Initialize(context);
            RtcEngine.InitEventHandler(handler);
            
            GameObject.Find("InitButton").GetComponent<Button>().onClick.Invoke();
        }

        private void SetBasicConfiguration()
        {
            RtcEngine.EnableAudio();
            RtcEngine.SetChannelProfile( CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING );
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
            
            var config = new AudioTrackConfig();
            trackId = RtcEngine.CreateCustomAudioTrack(AUDIO_TRACK_TYPE.AUDIO_TRACK_DIRECT, config);
        }

        #region -- Button Events ---

        public void MuteLocal(bool val)
        {
            RtcEngine.MuteLocalAudioStream(val);
        }

        public void DisableAudio(bool val)
        {
            if (val) RtcEngine.DisableAudio();
            else RtcEngine.EnableAudio();
        }
        
        public void JoinChannel()
        {
            var ret = RtcEngine.JoinChannel(_token, _channelName, "", 0);
            // PrepareScreenCapture();
            Debug.Log("JoinChannel returns: " + ret);
            
            // Debug.Log(RtcEngine.get);
        }

        public void LeaveChannel()
        {
            RtcEngine.LeaveChannel();
        }

        #endregion

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

        #region -- Video Render UI Logic ---

        internal static void MakeVideoView(uint uid, string channelId = "", VIDEO_SOURCE_TYPE videoSourceType = VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA)
        {
            var go = GameObject.Find(uid.ToString());
            if (!ReferenceEquals(go, null))
            {
                return; // reuse
            }
        
            // create a GameObject and assign to this new user
            var videoSurface = MakeImageSurface(uid.ToString());
            if (ReferenceEquals(videoSurface, null)) return;
            // configure videoSurface
            videoSurface.SetForUser(uid, channelId, videoSourceType);
            videoSurface.SetEnable(true);
        
            videoSurface.OnTextureSizeModify += (int width, int height) =>
            {
                var transform = videoSurface.GetComponent<RectTransform>();
                if (transform)
                {
                    //If render in RawImage. just set rawImage size.
                    transform.sizeDelta = new Vector2(width / 2, height / 2);
                    transform.localScale = videoSourceType == VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN ? new Vector3(-1, 1, 1) : Vector3.one;
                }
                else
                {
                    //If render in MeshRenderer, just set localSize with MeshRenderer
                    float scale = (float)height / (float)width;
                    videoSurface.transform.localScale = new Vector3(-1, 1, scale);
                }
                Debug.Log("OnTextureSizeModify: " + width + "  " + height);
            };
        }
        //
        // // VIDEO TYPE 1: 3D Object
        // private static VideoSurface MakePlaneSurface(string goName)
        // {
        //     var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        //
        //     if (go == null)
        //     {
        //         return null;
        //     }
        //
        //     go.name = goName;
        //     var mesh = go.GetComponent<MeshRenderer>();
        //     if (mesh != null)
        //     {
        //         Debug.LogWarning("VideoSureface update shader");
        //         mesh.material = new Material(Shader.Find("Unlit/Texture"));
        //     }
        //     // set up transform
        //     go.transform.Rotate(-90.0f, 0.0f, 0.0f);
        //     go.transform.position = Vector3.zero;
        //     go.transform.localScale = new Vector3(0.25f, 0.5f, .5f);
        //
        //     // configure videoSurface
        //     var videoSurface = go.AddComponent<VideoSurface>();
        //     return videoSurface;
        // }
        //
        // // Video TYPE 2: RawImage
        private static VideoSurface MakeImageSurface(string goName)
        {
            var go = new GameObject();
        
            if (go == null)
            {
                return null;
            }
        
            go.name = goName;
            // to be renderered onto
            go.AddComponent<RawImage>();
            // make the object draggable
            go.AddComponent<UIElementDrag>();
            var canvas = GameObject.Find("VideoCanvas");
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
            go.transform.localScale = new Vector3(3f, 4f, 1f);
        
            // configure videoSurface
            var videoSurface = go.AddComponent<VideoSurface>();
            return videoSurface;
        }

        #endregion
    }

    #region -- Agora Event ---

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly ScreenShareClient _desktopScreenShare;

        internal UserEventHandler(ScreenShareClient desktopScreenShare)
        {
            _desktopScreenShare = desktopScreenShare;
        }

        public override void OnError(int err, string msg)
        {
            _desktopScreenShare.Log.UpdateLog(string.Format("OnError err: {0}, msg: {1}", err, msg));
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            int build = 0;
            _desktopScreenShare.Log.UpdateLog(string.Format("sdk version: ${0}",
                _desktopScreenShare.RtcEngine.GetVersion(ref build)));
            _desktopScreenShare.Log.UpdateLog(
                string.Format("OnJoinChannelSuccess channelName: {0}, uid: {1}, elapsed: {2}",
                                connection.channelId, connection.localUid, elapsed));
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _desktopScreenShare.Log.UpdateLog("OnRejoinChannelSuccess");
        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _desktopScreenShare.Log.UpdateLog("OnLeaveChannel");
            // ScreenShareClient.DestroyVideoView(connection.localUid);
        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            Debug.Log("Old role" + oldRole.ToString());
            _desktopScreenShare.Log.UpdateLog("OnClientRoleChanged");
            Debug.Log("New role" + newRole.ToString());
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _desktopScreenShare.Log.UpdateLog(string.Format("OnUserJoined uid: ${0} elapsed: ${1}", uid, elapsed));
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _desktopScreenShare.Log.UpdateLog(string.Format("OnUserOffLine uid: ${0}, reason: ${1}", uid,
                (int)reason));
        }
        
        public override void OnStreamMessage(RtcConnection connection, uint remoteUid, int streamId, byte[] data, ulong length, ulong sentTs)
        {
            string streamMessage = System.Text.Encoding.Default.GetString(data, 0, (int)length);
            Debug.Log($"OnStreamMessage - Usuário Remoto: {remoteUid}, Mensagem: {streamMessage}");
            AgoraSignalManager.Instance.OnStreamMessage(remoteUid, streamId, data, (int)length);
        }
    }

    #endregion 
}
