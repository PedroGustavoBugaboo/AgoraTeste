using Agora.Rtc;
using UnityEngine;

public class MediaController : MonoBehaviour
{
    private bool camera;
    private bool microfone = true;
    
    public IRtcEngine RtcEngine;

    private void Awake()
    {
        ToggleMicrophone();
    }
    
    public void ToggleCamera()
    {
        if (RtcEngine == null)
        {
            RtcEngine = Agora.Rtc.RtcEngine.Get();
            if(RtcEngine != null) Debug.Log("Pegou o rtc");
            else
            {
                Debug.Log("Rtc está nulo");
                return;
            }
        }

        camera = !camera;
        
        if (camera)
        {
            RtcEngine.EnableVideo();
        }
        else
        {
            RtcEngine.DisableVideo();
        }
    }

    public void ToggleMicrophone()
    {
        if (RtcEngine == null)
        {
            RtcEngine = Agora.Rtc.RtcEngine.Get();
            if(RtcEngine != null) Debug.Log("Pegou o rtc");
            else
            {
                Debug.Log("Rtc está nulo");
                return;
            }
        }

        microfone = !microfone;
        
        if (microfone)
        {
            int result = RtcEngine.MuteLocalAudioStream(false);
            if (result != 0)
            {
                Debug.LogError($"Falha ao desmutar o microfone. Código de erro: {result}");
            }
            else
            {
                Debug.Log("Microfone desmutado com sucesso.");
            }
            
            RtcEngine.EnableAudioVolumeIndication(200, 3, true);
        }
        else
        {
            RtcEngine.MuteLocalAudioStream(true);
        }
    }
}
