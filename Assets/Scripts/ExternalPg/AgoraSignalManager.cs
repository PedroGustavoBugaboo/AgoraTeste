using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agora.Rtc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgoraSignalManager : MonoBehaviour
{
    private static AgoraSignalManager instance;
    public static AgoraSignalManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AgoraSignalManager>();
            }

            return instance;
        }
    }
    
    public TextMeshProUGUI text;
    public MonoBehaviour script;

    [SerializeField] private Button unmuteUserBtn;
    [SerializeField] private Toggle muteMic;

    private List<uint> awakeUsers;
    private uint currentIdVoice = 0;
    private bool admControl => currentIdVoice == 0;
    
    private IRtcEngine _engine;
    private int streamId;

    private void Awake()
    {
        awakeUsers = new List<uint>();
    }

    public void GetRtc()
    {
        var type = script.GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            Debug.Log(field.Name);

            // Verifica se o campo é o que você está procurando
            if (field.Name == "RtcEngine")
            {
                Debug.Log($"Campo do tipo IRtcEngine encontrado: {field.Name}");

                // Obtém o valor do campo
                var fieldValue = field.GetValue(script);

                // Verifica se o valor é do tipo IRtcEngine
                if (fieldValue is IRtcEngine engine)
                {
                    _engine = engine;
                    Debug.Log($"Campo do tipo IRtcEngine encontrado e atribuído: {_engine}");
                }
                else
                {
                    Debug.LogWarning("O valor do campo encontrado não é do tipo IRtcEngine.");
                }

                // Interrompe o loop após encontrar e atribuir o campo
                break;
            }
        }
    }

    public void SendMessage(string message)
    {
        // Cria um data stream se ainda não tiver sido criado
        if (streamId == 0)
        {
            int result = _engine.CreateDataStream(ref streamId, true, true);
            if (result != 0)
            {
                Debug.LogError("Falha ao criar Data Stream: " + result);
                return;
            }
        }

        // Converte a mensagem em bytes
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
        int sendResult = _engine.SendStreamMessage(streamId, messageBytes, Convert.ToUInt32(messageBytes.Length));

        if (sendResult != 0)
        {
            Debug.LogError("Falha ao enviar mensagem: " + sendResult);
        }
        else
        {
            Debug.Log("Mensagem enviada com sucesso.");
        }
    }
    
    public void OnStreamMessage(uint remoteUid, int streamId, byte[] data, int length)
    {
        string message = System.Text.Encoding.UTF8.GetString(data, 0, length);
        text.text = $"Recebido do usuário {remoteUid} : {message}";
        UpdateAwakeList(remoteUid);
    }

    private void UpdateAwakeList(uint remoteUid)
    {
        if(awakeUsers.Count > 0 && awakeUsers.Any(c => c == remoteUid)) return;
        awakeUsers.Add(remoteUid);
        
        UpdateUi();
    }

    public void PressUnmutButton()
    {
        if(admControl) UserUnmuted();
        else RetakeMic();
    }
    
    private void UserUnmuted()
    {
        currentIdVoice = awakeUsers.First();
        _engine.MuteRemoteAudioStream(currentIdVoice, false);
        awakeUsers.Remove(currentIdVoice);
        
        UpdateUi();
    }

    private void RetakeMic()
    {
        _engine.MuteRemoteAudioStream(currentIdVoice, true);
        currentIdVoice = 0;
        
        UpdateUi();
    }

    private void UpdateUi()
    {
        //Mais de um usuário esperando
        if (awakeUsers.Count > 0)
        {
            unmuteUserBtn.interactable = true;
            
            if (admControl)
            {
                muteMic.interactable = true;
                unmuteUserBtn.GetComponentInChildren<TextMeshProUGUI>().text = $"Unmute user {awakeUsers.First()}";
            }
            else
            {
                muteMic.interactable = false;
                unmuteUserBtn.GetComponentInChildren<TextMeshProUGUI>().text = $"Retake mic";
            }
        }
        else
        {
            if (admControl)
            {
                unmuteUserBtn.interactable = false;
                muteMic.interactable = true;
            }
            else
            {
                muteMic.interactable = false;
                unmuteUserBtn.GetComponentInChildren<TextMeshProUGUI>().text = $"Retake mic";
            }
        }
    }
}
