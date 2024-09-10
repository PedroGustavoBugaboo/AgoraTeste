using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "VirtualMeeting/RoomSetup", fileName = "RoomSetup", order = 1)]
[Serializable]
public class AgoraRoomSetup : ScriptableObject
{
    [FormerlySerializedAs("APP_ID")]
    [SerializeField]
    public string appID = "";

    [FormerlySerializedAs("TOKEN")]
    [SerializeField]
    public string token = "";

    [FormerlySerializedAs("CHANNEL_NAME")]
    [SerializeField]
    public string channelName = "YOUR_CHANNEL_NAME";
}
