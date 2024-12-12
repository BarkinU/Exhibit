using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = System.Object;


public class PlayerUI : MonoBehaviour
{
    [Header("Player Panels")] [SerializeField]
    private GameObject _friendResponseUI,
        _videoCallResponseAndTriggerUI,
        _videoCallInviteResponseUI,
        _videoConferenceJoinResponseUI,
        _videoConferenceCameraUI;

    [Header("Video Call")] public Button connectVideoCallButton;
    public GridLayoutGroup ViewContainer;
    public RawImage ViewPrefab;
    public Button disconnectButton;
    private VideoCallHandler _videoCallHandler;

    [Header("Send Request")] public Button sendFriendRequestButton;
    public Button sendVideoCallInviteButton;
    public Button sendCreateVideoCallInviteButton;
    public Button sendJoinRequestButton;

    [Header("Interaction UI")] public GameObject InteractionUI;
    public bool _isMessageInputFieldSelected;


    public TMP_InputField userChatMessageInputField;
    [HideInInspector] public TMP_Text userChatMessageInputFieldText;

    private string _userChatMessageText;


    [Header("Message UI")] [SerializeField]
    private NetworkInGameMessages networkInGameMessages;

    [SerializeField] private NetworkPlayer _networkPlayer;
    public ChatState currentChatState;

    [Header("Animation UI Buttons")] [SerializeField]
    private GameObject emotionAnimPanel;

    [SerializeField] private GameObject greetingsAnimPanel;
    [SerializeField] private GameObject danceAnimPanel;

    [Header("Animation UI Buttons")] public GameObject wantToSitPanel;


    private void Start()
    {
        userChatMessageInputFieldText =
            userChatMessageInputField.transform.Find("Text Area/Text").GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (_isMessageInputFieldSelected)
        {
            if (Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return))
            {
                if (!string.IsNullOrEmpty(userChatMessageInputField.text))
                {
                    SendMessageDependingChatState();
                }
            }
        }
    }


    public void OpenFriendResponseUI()
    {
        _friendResponseUI.SetActive(true);
    }

    public void OpenVideoCallUIAndTriggerTargetPlayer()
    {
        _videoCallResponseAndTriggerUI.SetActive(true);
    }

    public void OpenVideoCallInviteResponseUI()
    {
        _videoCallInviteResponseUI.SetActive(true);
    }


    public void OpenVideoCallJoinResponseUI()
    {
        _videoConferenceJoinResponseUI.SetActive(true);
    }

    public void OpenVideoConferenceCameraUI()
    {
        _videoConferenceCameraUI.SetActive(true);
    }

    public void CloseVideoConferenceCameraUI()
    {
        _videoConferenceCameraUI.SetActive(false);
    }


    public void WhenInputFieldSelected()
    {
        _isMessageInputFieldSelected = true;
    }

    public void WhenInputFieldDeselected()
    {
        _isMessageInputFieldSelected = false;
    }


    // ReSharper disable Unity.PerformanceAnalysis
    public void SendMessageDependingChatState()
    {
        switch (currentChatState)
        {
            case ChatState.Global:
                _userChatMessageText = userChatMessageInputFieldText.text;
                networkInGameMessages.RPC_SendGlobalMessageInputToStateAuthority(_networkPlayer.NickName.ToString(),
                    _userChatMessageText, currentChatState);
                userChatMessageInputField.text = "";
                break;
            case ChatState.Group:
                // for (int i = 0; i < _networkPlayer.myParty.playersInParty.Count; i++)
                // {
                //     _userChatMessageText = userChatMessageInputFieldText.text;
                //     networkInGameMessages.Rpc_SendGroupMessageToTargetPlayers(_networkPlayer.myParty.playersInParty[i],
                //         _networkPlayer.NickName.ToString(),
                //         _userChatMessageText, currentChatState);
                // }
                //
                // userChatMessageInputField.text = "";

                _userChatMessageText = userChatMessageInputFieldText.text;
                networkInGameMessages.RPC_SendGlobalMessageInputToStateAuthority(_networkPlayer.NickName.ToString(),
                    _userChatMessageText, currentChatState);
                userChatMessageInputField.text = "";

                break;
            case ChatState.Normal:
                // _userChatMessageText = userChatMessageInputFieldText.text;
                // networkInGameMessages.RPC_SendGlobalMessageInputToStateAuthority(_networkPlayer.NickName.ToString(),
                //     _userChatMessageText, currentChatState);
                // userChatMessageInputField.text = "";

                _userChatMessageText = userChatMessageInputFieldText.text;
                networkInGameMessages.RPC_SendGlobalMessageInputToStateAuthority(_networkPlayer.NickName.ToString(),
                    _userChatMessageText, currentChatState);
                userChatMessageInputField.text = "";
                break;
        }
    }

    public void OpenCloseEmotionAnimationPanel()
    {
        emotionAnimPanel.SetActive(!emotionAnimPanel.activeSelf);
    }

    public void OpenCloseGreetingsAnimationPanel()
    {
        greetingsAnimPanel.SetActive(!greetingsAnimPanel.activeSelf);
    }

    public void OpenCloseDanceAnimationPanel()
    {
        danceAnimPanel.SetActive(!danceAnimPanel.activeSelf);
    }

    public void CloseAllPanels()
    {
        emotionAnimPanel.SetActive(false);
        greetingsAnimPanel.SetActive(false);
        danceAnimPanel.SetActive(false);
    }
}