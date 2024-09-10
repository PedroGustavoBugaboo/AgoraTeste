using Agora.Rtc;
using UnityEngine;

public class AgoraAudioSetup : MonoBehaviour
{
    // private IRtcEngine _rtcEngine;
    // private CustomAudioSource _customAudioSource;
    //
    // void Start()
    // {
    //     _rtcEngine = RtcEngine.CreateAgoraRtcEngine();
    //     _rtcEngine.Initialize(new RtcEngineContext { appId = "YOUR_APP_ID" });
    //
    //     _customAudioSource = new CustomAudioSource(bufferSize: 1024);
    //
    //     // Configura a captura de áudio customizado
    //     _rtcEngine.EnableCustomAudioTrack();
    //     _rtcEngine.SetAudioFrameObserver(_customAudioSource);
    //
    //     // Publique a fonte de áudio customizada
    //     _rtcEngine.PublishCustomAudioTrack(_customAudioSource);
    // }
    //
    // void OnDestroy()
    // {
    //     _rtcEngine.Stop();
    //     _rtcEngine.Dispose();
    // }
}
