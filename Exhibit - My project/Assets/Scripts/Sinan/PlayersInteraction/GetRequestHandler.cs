using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using Random = UnityEngine.Random;


public class GetRequestHandler : NetworkBehaviour
{
    NetworkPlayer networkPlayer;
    public OutgoingRequestType outgoingRequestType;
    public IncomingRequestType incomingRequestType;
    private VideoCallHandler _videoCallHandler;
    public int tempRoomId;
    public PlayerRef tempTargetPlayer;
    public PlayerRef tempVideoCallCreator;


    [Header("Player UI")] private PlayerUI playerUI;

    private void Awake()
    {
        _videoCallHandler = GetComponent<VideoCallHandler>();
        networkPlayer = GetComponent<NetworkPlayer>();
        playerUI = GetComponentInChildren<PlayerUI>();
    }


    public void ShowUIForVideoCallRequestType(OutgoingRequestType incomingOutgoingRequestType,
        PlayerRef senderOfRequest, int roomId)
    {
        tempRoomId = roomId;
        tempTargetPlayer = senderOfRequest;

        Debug.Log(
            $"Room ID: {roomId}, Target: {senderOfRequest}, nickName: {GetComponent<NetworkPlayer>().NickName}");
        ChangeOutgoingRequestType(incomingOutgoingRequestType);
        GetOutgoingRequestsResponses();
    }

    public void ShowUIForJoinRequestType(OutgoingRequestType incomingOutgoingRequestType, PlayerRef senderOfRequest)
    {
        tempTargetPlayer = senderOfRequest;
        ChangeOutgoingRequestType(incomingOutgoingRequestType);
        GetOutgoingRequestsResponses();
    }

    public void SendVideoCallAccepted(bool acceptResponse, IncomingRequestType incomingOutgoingRequestType)
    {
        if (acceptResponse)
        {
            ChangeIncoingRequestType(incomingOutgoingRequestType);
            GetIncomingRequestsResponses();
        }
        else
        {
            Debug.Log($"Request denied");
        }
    }


    private void GetOutgoingRequestsResponses()
    {
        switch (outgoingRequestType)
        {
            case OutgoingRequestType.FriendRequest:
                playerUI.OpenFriendResponseUI();
                break;
            case OutgoingRequestType.CreateRoomThenVideoCallRequest:
                playerUI.OpenVideoCallUIAndTriggerTargetPlayer();
                break;
            case OutgoingRequestType.VideoCallInvite:
                playerUI.OpenVideoCallInviteResponseUI();
                break;
            case OutgoingRequestType.JoinVideoCallRequest:
                playerUI.OpenVideoCallJoinResponseUI();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(outgoingRequestType), outgoingRequestType, null);
        }
    }

    private void GetIncomingRequestsResponses()
    {
        switch (incomingRequestType)
        {
            case IncomingRequestType.VideoCallResponse:
                _videoCallHandler._videoCallInvitedOrCreated = false;
                _videoCallHandler.StartVideoConferenceForClickInvite();
                networkPlayer.Rpc_AddMyPlayerRefToServerList(networkPlayer.currentRoomId, Object.InputAuthority);
                Debug.LogWarning(
                    $"Player Joined to room {networkPlayer.currentRoomId} playerAdded to the party");

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(outgoingRequestType), outgoingRequestType, null);
        }
    }

    public void AcceptVideoCallInviteButton()
    {
        _videoCallHandler.DisconnectFromCurrentRoom();
        networkPlayer.Rpc_SetMyRoomIdFromStateAuthority(tempRoomId);
        networkPlayer.Rpc_ChangeMyTargetFromStateAuthority(tempTargetPlayer);
        _videoCallHandler._videoCallInvitedOrCreated = false;
        GetRoomTokenFromNetwork();
    }

    public void AcceptAndTriggerTargetPlayerCamera()
    {
        _videoCallHandler.DisconnectFromCurrentRoom();
        networkPlayer.Rpc_SetMyRoomIdFromStateAuthority(tempRoomId);
        networkPlayer.Rpc_ChangeMyTargetFromStateAuthority(tempTargetPlayer);
        // networkPlayer.Rpc_ChangeVideoCallCreatorFromState(tempTargetPlayer);
        // _videoCallHandler.AddPlayerToCreatorsGroupList(tempTargetPlayer);


        _videoCallHandler._videoCallInvitedOrCreated = true;
        GetRoomTokenFromNetwork();
    }


    public void AcceptJoinRequest()
    {
        _videoCallHandler.TriggerJoinedTargetPlayerVideoConference();
    }


    private void GetRoomTokenFromNetwork()
    {
        Singleton.Main.NetworkManager.GetRoomToken(tempRoomId.ToString(),
            networkPlayer.NickName.ToString(), GetRoomTokenCallback);
    }

    void GetRoomTokenCallback(string data)
    {
        var json = JsonUtility.FromJson<RoomToken>(data);
        VideoCallHandler.RoomToken = json.token;
        _videoCallHandler.StartVideoConferenceForClickInvite();
        networkPlayer.Rpc_AddMyPlayerRefToServerList(tempRoomId, Object.InputAuthority);
        Debug.LogWarning(
            $"Player Joined to room {tempRoomId} playerAdded to the party");
    }

    

    public void ChangeOutgoingRequestType(OutgoingRequestType tempOutgoingRequestType)
    {
        outgoingRequestType = tempOutgoingRequestType;
        Debug.Log($"Gelen istek tipi :{outgoingRequestType}");
    }

    public void ChangeIncoingRequestType(IncomingRequestType tempIncomingRequestType)
    {
        incomingRequestType = tempIncomingRequestType;
        Debug.Log($"Gelen istek tipi :{outgoingRequestType}");
    }
}