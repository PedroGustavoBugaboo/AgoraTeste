using UnityEngine;

[CreateAssetMenu(fileName = "DataLogin", menuName = "Data/DataLogin")]
public class DataLogin : ScriptableObject
{
    public string _appID = "";
    public string _token = "";
    public string _channelName = "";
}
