using System.Collections;
using System.Collections.Generic;
using Agora_RTC_Plugin.API_Example;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public TMP_InputField AppIdInupt;
    public TMP_InputField ChannelInput;
    public TMP_InputField TokenInput;
    
    public AgoraRoomSetup AppInputConfig;
    public GameObject CasePanel;
    public GameObject CaseScrollerView;
    
    public GameObject EventSystem;

    private string _playSceneName = "";

    private string[] _rtcNameList =
    {
        "Room1",
        "Room2",
    };

    private void Awake()
    {
        PermissionHelper.RequestMicrophontPermission();
        PermissionHelper.RequestCameraPermission();
        
        GameObject content = GameObject.Find("Content");
        for (int i = 0; i < _rtcNameList.Length; i++)
        {
            var go = Instantiate(CasePanel, content.transform);
            var props = go.GetComponent<EnterValueProp>();
            props.Text.text = _rtcNameList[i];
            props.Button.onClick.AddListener(OnJoinSceneClicked);
            props.Button.onClick.AddListener(SetScolllerActive);
        }
        
        if (AppInputConfig != null)
        {
            AppIdInupt.text = AppInputConfig.appID;
            ChannelInput.text = AppInputConfig.channelName;
            TokenInput.text = AppInputConfig.token;
        }
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
        if (_playSceneName != "")
        {
            AsyncOperation async = SceneManager.UnloadSceneAsync(_playSceneName);
            yield return async;
            EventSystem.gameObject.SetActive(true);
        }
    }

    public void OnJoinSceneClicked()
    {
        AppInputConfig.appID = AppIdInupt.text;
        AppInputConfig.channelName = ChannelInput.text;
        AppInputConfig.token = TokenInput.text;

        var button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        var sceneName = button.GetComponentInParent<EnterValueProp>().Text.text;

        EventSystem.gameObject.SetActive(false);

        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        _playSceneName = sceneName;

    }

    public void SetScolllerActive()
    {
        CaseScrollerView.SetActive(false);
    }
}
