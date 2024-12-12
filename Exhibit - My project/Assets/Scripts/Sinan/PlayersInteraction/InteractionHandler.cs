using UnityEngine;
using Fusion;
using LiveKit;
using System.Collections;


public enum OutgoingRequestType
{
    CreateRoomThenVideoCallRequest,
    VideoCallInvite,
    JoinVideoCallRequest,
    FriendRequest,
    MessageRequest
}

public enum IncomingRequestType
{
    VideoCallResponse,
    JoinVideoCallResponse
}

public class InteractionHandler : NetworkBehaviour
{
    [Header("Collision")] public LayerMask collisionLayers;

    [Networked(OnChanged = nameof(OnVideoCallAccepted))]
    public bool isVideoCallAccepted { get; set; }

    [Networked(OnChanged = nameof(OnVideoCallJoinRequestAccepted))]
    public bool isJoinRequestAccepted { get; set; }


    public bool isEnteredRoom { get; set; }

    //
    // [Networked(OnChanged = nameof(OnCharacterQuitToTheVideoCall))]
    // public bool isCharacterQuitToTheVideoCall { get; set; }

    public PlayerRef tempAddablePlayer;

    private VideoCallHandler _videoCallHandler;

    float lastTimeFired = 0;


    //Other Components
    public NetworkPlayer networkPlayer;
    private IRequest _request;
    private OutgoingRequestType _outgoingRequestType;
    private string messageContent;
    [Header("Player UI")] private PlayerUI playerUI;
    private PlayerRef _myTargetPlayerRef;
    private bool isJoinRequestOrInvite;

    [Header("Chracter Interaction")] private LagCompensatedHit _lastHit;

    [SerializeField] private Camera myCamera;
    private int tempRoomId;

    private void Awake()
    {
        playerUI = GetComponentInChildren<PlayerUI>();
        networkPlayer = GetComponent<NetworkPlayer>();
        _videoCallHandler = GetComponent<VideoCallHandler>();
    }

    private void Start()
    {
        playerUI.sendFriendRequestButton.onClick.AddListener(FriendRequestListener);
        playerUI.sendVideoCallInviteButton.onClick.AddListener(InviteRequestListener);
        playerUI.sendCreateVideoCallInviteButton.onClick.AddListener(VideoCallRequestListener);
        playerUI.sendJoinRequestButton.onClick.AddListener(JoinRequestListener);
    }


    public override void FixedUpdateNetwork()
    {
        // //Get the input from the network
        // if (GetInput(out NetworkInputData networkInputData))
        // {
        //     if (networkInputData.IsCharacterSelectButtonPressed)
        //     {
        //         if (Object.InputAuthority)
        //             SelectPlayer();
        //     }
        // }
    }

    void SelectPlayer()
    {
        Vector3 screenMousePosFar = new Vector3(Input.mousePosition.x, Input.mousePosition.y, myCamera.farClipPlane);
        Vector3 screenMousePosNear =
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, myCamera.nearClipPlane);
        Vector3 worldMousePosFar = myCamera.ScreenToWorldPoint(screenMousePosFar);
        Vector3 worldMousePosNear = myCamera.ScreenToWorldPoint(screenMousePosNear);

        Runner.LagCompensation.Raycast(worldMousePosNear, worldMousePosFar - worldMousePosNear, 100,
            Object.InputAuthority, out var hitInfo,
            collisionLayers, HitOptions.IgnoreInputAuthority);

        float hitDistance = 100;
        bool isHitOtherPlayer = false;

        if (hitInfo.Distance > 0)
            hitDistance = hitInfo.Distance;

        if (hitInfo.Hitbox != null)
        {
            if (Object.InputAuthority)
            {
                if (hitInfo.Hitbox.transform.root.GetComponent<GetRequestHandler>() != null)
                {
                    var root = hitInfo.Hitbox.transform.root;
                    _myTargetPlayerRef = root.GetComponent<NetworkPlayer>().Object
                        .InputAuthority;
                    ButtonsStates();
                    OpenInteractionUI();
                    isHitOtherPlayer = true;
                }
            }
        }
        else if (hitInfo.Collider != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit PhysX collider {hitInfo.Collider.transform.name}");
        }

        //Debug
        if (isHitOtherPlayer)
            Debug.DrawRay(worldMousePosNear, (worldMousePosFar - worldMousePosNear) * hitDistance, Color.red, 1);
        else
            Debug.DrawRay(worldMousePosNear, (worldMousePosFar - worldMousePosNear) * hitDistance, Color.green, 1);

        lastTimeFired = Time.time;
    }

    #region OnChanged Methods

    static void OnVideoCallAccepted(Changed<InteractionHandler> changed)
    {
        bool videoCallAcceptCurrent = changed.Behaviour.isVideoCallAccepted;

        if (videoCallAcceptCurrent)
            changed.Behaviour.AcceptVideoCallRequest();
    }

    static void OnVideoCallJoinRequestAccepted(Changed<InteractionHandler> changed)
    {
        bool JoinRequestAcceptedCurrent = changed.Behaviour.isJoinRequestAccepted;


        if (JoinRequestAcceptedCurrent)
            changed.Behaviour.JoinRequestAccepted();
    }

    #endregion


    private void OpenInteractionUI()
    {
        playerUI.InteractionUI.SetActive((true));
    }


    private void SendInvite()
    {
        ChangeRequestType();
    }

    private void CreateRoomIdThenSendToStateAuthority()
    {
        tempRoomId = Utils.GenerateRandomNumber();
        GetComponent<NetworkPlayer>().Rpc_SetMyRoomIdFromStateAuthority(tempRoomId);
    }


    private void GetRoomTokenForVideoCall()
    {
        Singleton.Main.NetworkManager.GetRoomToken(tempRoomId.ToString(),
            networkPlayer.NickName.ToString(), GetRoomTokenCallback);
    }

    private void JoinRequestsResponse()
    {
        Singleton.Main.NetworkManager.GetRoomToken(tempRoomId.ToString(),
            networkPlayer.NickName.ToString(), GetRoomTokenCallbackForIncoming);
        GetComponent<NetworkPlayer>().Rpc_SetMyRoomIdFromStateAuthority(tempRoomId);
    }


    #region SendRequestWithRPCsToClients

    // Open Selected Character UI for Video Conference
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void Rpc_SendVideoCallRequestToClient([RpcTarget] PlayerRef player,
        OutgoingRequestType tempOutgoingRequestType,
        PlayerRef playerRef, int roomID)
    {
        if (Runner.TryGetPlayerObject(player, out var plObject))
        {
            plObject.GetComponent<GetRequestHandler>()
                .ShowUIForVideoCallRequestType(tempOutgoingRequestType, playerRef, roomID);
        }
    }

    // Open Selected Character UI for Join Request
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void Rpc_SendJoinToVideoCallRequestToClient([RpcTarget] PlayerRef player,
        OutgoingRequestType tempOutgoingRequestType,
        PlayerRef playerRef)
    {
        if (Runner.TryGetPlayerObject(player, out var plObject))
        {
            plObject.GetComponent<GetRequestHandler>()
                .ShowUIForJoinRequestType(tempOutgoingRequestType, playerRef);
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void Rpc_ChangeVideoCallResponse([RpcTarget] PlayerRef player)
    {
        if (Runner.TryGetPlayerObject(player, out var plObject))
        {
            plObject.GetComponent<InteractionHandler>()
                .isVideoCallAccepted = true;
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void Rpc_JoinToVideoCallResponse([RpcTarget] PlayerRef player, int roomID)
    {
        if (Runner.TryGetPlayerObject(player, out var plObject))
        {
            plObject.GetComponent<InteractionHandler>()
                .tempRoomId = roomID;
            plObject.GetComponent<InteractionHandler>()
                .isJoinRequestAccepted = true;
        }
    }

    #endregion


    // ReSharper disable Unity.PerformanceAnalysis
    void GetRoomTokenCallback(string data)
    {
        var json = JsonUtility.FromJson<RoomToken>(data);
        VideoCallHandler.RoomToken = json.token;
    }

    void GetRoomTokenCallbackForIncoming(string data)
    {
        var json = JsonUtility.FromJson<RoomToken>(data);
        VideoCallHandler.RoomToken = json.token;
        GetComponent<GetRequestHandler>().SendVideoCallAccepted(true, IncomingRequestType.VideoCallResponse);
    }


    private void ChangeRequestType()
    {
        OutgoingRequestType tempOutgoingRequestType = OutgoingRequestType.VideoCallInvite;
        Runner.TryGetPlayerObject(_myTargetPlayerRef, out var plObject);

        if (networkPlayer.playerCurrentState == PlayerState.Idle)
        {
            switch (plObject.GetComponent<NetworkPlayer>().playerCurrentState)
            {
                case PlayerState.Idle:

                    tempOutgoingRequestType = OutgoingRequestType.CreateRoomThenVideoCallRequest;
                    CreateRoomIdThenSendToStateAuthority();
                    GetRoomTokenForVideoCall();
                    Rpc_SendVideoCallRequestToClient(_myTargetPlayerRef, tempOutgoingRequestType, Object.InputAuthority,
                        tempRoomId);
                    break;
                case PlayerState.InVideoCall:
                    tempOutgoingRequestType = OutgoingRequestType.JoinVideoCallRequest;
                    tempRoomId = plObject.GetComponent<NetworkPlayer>().currentRoomId;
                    Rpc_SendJoinToVideoCallRequestToClient(_myTargetPlayerRef, tempOutgoingRequestType,
                        Object.InputAuthority);
                    break;
            }
        }
        else if (networkPlayer.playerCurrentState == PlayerState.InVideoCall)
        {
            switch (plObject.GetComponent<NetworkPlayer>().playerCurrentState)
            {
                case PlayerState.Idle:
                    tempOutgoingRequestType = OutgoingRequestType.VideoCallInvite;
                    Rpc_SendVideoCallRequestToClient(_myTargetPlayerRef, tempOutgoingRequestType, Object.InputAuthority,
                        networkPlayer.currentRoomId);
                    break;
                case PlayerState.InVideoCall:

                    if (isJoinRequestOrInvite)
                    {
                        _videoCallHandler.DisconnectFromCurrentRoom();
                        tempOutgoingRequestType = OutgoingRequestType.JoinVideoCallRequest;
                        Rpc_SendJoinToVideoCallRequestToClient(_myTargetPlayerRef, tempOutgoingRequestType,
                            Object.InputAuthority);
                    }
                    else
                    {
                        tempOutgoingRequestType = OutgoingRequestType.VideoCallInvite;
                        Rpc_SendVideoCallRequestToClient(_myTargetPlayerRef, tempOutgoingRequestType,
                            Object.InputAuthority,
                            networkPlayer.currentRoomId);
                    }

                    isJoinRequestOrInvite = false;

                    break;
            }
        }
    }


    private void ButtonsStates()
    {
        Runner.TryGetPlayerObject(_myTargetPlayerRef, out var plObject);

        if (networkPlayer.playerCurrentState == PlayerState.Idle)
        {
            switch (plObject.GetComponent<NetworkPlayer>().playerCurrentState)
            {
                case PlayerState.Idle:
                    playerUI.sendCreateVideoCallInviteButton.gameObject.SetActive(true);
                    playerUI.sendVideoCallInviteButton.gameObject.SetActive(false);
                    playerUI.sendJoinRequestButton.gameObject.SetActive(false);
                    break;
                case PlayerState.InVideoCall:
                    playerUI.sendCreateVideoCallInviteButton.gameObject.SetActive(false);
                    playerUI.sendVideoCallInviteButton.gameObject.SetActive(false);
                    playerUI.sendJoinRequestButton.gameObject.SetActive(true);
                    break;
            }
        }
        else if (networkPlayer.playerCurrentState == PlayerState.InVideoCall)
        {
            switch (plObject.GetComponent<NetworkPlayer>().playerCurrentState)
            {
                case PlayerState.Idle:
                    playerUI.sendCreateVideoCallInviteButton.gameObject.SetActive(false);
                    playerUI.sendVideoCallInviteButton.gameObject.SetActive(true);
                    playerUI.sendJoinRequestButton.gameObject.SetActive(false);
                    break;
                case PlayerState.InVideoCall:
                    playerUI.sendCreateVideoCallInviteButton.gameObject.SetActive(false);
                    playerUI.sendVideoCallInviteButton.gameObject.SetActive(true);
                    playerUI.sendJoinRequestButton.gameObject.SetActive(true);
                    break;
            }
        }
    }

    #region OnChanged Responses

    private void AcceptVideoCallRequest()
    {
        GetComponent<GetRequestHandler>().SendVideoCallAccepted(true, IncomingRequestType.VideoCallResponse);
        isVideoCallAccepted = false;
    }

    private void JoinRequestAccepted()
    {
        JoinRequestsResponse();
        isJoinRequestAccepted = false;
    }

    // private void AddCharacterToTheList()
    // {
    //     networkPlayer.Rpc_AddPlayerRefToTheGroupList(tempAddablePlayer);
    //     isCharacterEnterToTheVideoCall = false;
    // }
    //
    // private void RemoveCharacterToTheList()
    // {
    //     networkPlayer.Rpc_RemovePlayerRefToTheGroupList(tempAddablePlayer);
    //     isCharacterQuitToTheVideoCall = false;
    // }

    #endregion

    #region RequestTypeListeners

    private void FriendRequestListener()
    {
        _request = new FriendRequest();
        messageContent =
            _request.ChangeRequest(messageContent, networkPlayer.NickName.ToString(), _outgoingRequestType);
        SendInvite();
    }

    private void JoinRequestListener()
    {
        isJoinRequestOrInvite = true;
        VideoCallRequestListener();
    }

    private void InviteRequestListener()
    {
        isJoinRequestOrInvite = false;
        VideoCallRequestListener();
    }

    private void VideoCallRequestListener()
    {
        _request = new VideoCallRequest();
        messageContent =
            _request.ChangeRequest(messageContent, networkPlayer.NickName.ToString(), _outgoingRequestType);
        SendInvite();
    }


    private void MessageRequestListener()
    {
        _request = new MessageRequest();
        messageContent =
            _request.ChangeRequest(messageContent, networkPlayer.NickName.ToString(), _outgoingRequestType);
        SendInvite();
    }


    public interface IRequest
    {
        string ChangeRequest(string message, string nickName, OutgoingRequestType outgoingRequestType);
    }

    public class VideoCallRequest : IRequest
    {
        public string ChangeRequest(string message, string nickName, OutgoingRequestType outgoingRequestType)
        {
            message = $"{nickName} send to you video call Request";
            outgoingRequestType = OutgoingRequestType.CreateRoomThenVideoCallRequest;
            return message;
        }
    }

    public class FriendRequest : IRequest
    {
        public string ChangeRequest(string message, string nickName, OutgoingRequestType outgoingRequestType)
        {
            message = $"{nickName} send to you friend Request";
            outgoingRequestType = OutgoingRequestType.FriendRequest;
            return message;
        }
    }

    public class MessageRequest : IRequest
    {
        public string ChangeRequest(string message, string nickName, OutgoingRequestType outgoingRequestType)
        {
            message = $"{nickName} send to you friend Request";
            outgoingRequestType = OutgoingRequestType.MessageRequest;
            return message;
        }
    }

    #endregion

    #region InteractionWithTrigger

    public void CheckMyParticipantListAndActivate(string participantIdentity)
    {
        Debug.Log($"Camera Activated");
        var otherCharacterCamera = GetCamera(participantIdentity);
        if (otherCharacterCamera != null)
        {
            otherCharacterCamera.myUsername.text = participantIdentity;
            ActivateIncomingCamera(otherCharacterCamera);
        }


        //MyCamera
        //ActivateMyLocalParticipantCameraAndMic();
    }

    public void CheckMyParticipantListAndDeActivate(string participantIdentity)
    {
        Debug.Log($"Camera Deactivated");
        var otherCharacterCamera = GetCamera(participantIdentity);
        if (otherCharacterCamera != null)
        {
            DeActivateIncomingCamera(otherCharacterCamera);
        }

        GetComponent<CharacterDetection>().charactersInRangeOldList.Remove(participantIdentity);

        //MyCamera
        if (GetComponent<CharacterDetection>().charactersInRangeOldList.Count > 0) return;
        //  DeActivateMyLocalParticipantCameraAndMic();
        // var myVideoCamera = GetCamera(_videoCallHandler.myLocalParticipantIdentity);
        // if (myVideoCamera == null) return;
        // DeActivateIncomingCamera(myVideoCamera);
        // playerUI.CloseVideoConferenceCameraUI();
    }

    public ParticipantCamera GetCamera(string participantIdentity)
    {
        _videoCallHandler._participantCameras.TryGetValue(participantIdentity, out var participantCamera);
        return participantCamera;
    }

    private void ActivateIncomingCamera(ParticipantCamera participantCamera)
    {
        if (participantCamera != null) participantCamera.gameObject.SetActive(true);
        Debug.LogWarning($"{participantCamera.name} opened camera");
    }

    private void DeActivateIncomingCamera(ParticipantCamera participantCamera)
    {
        if (participantCamera != null) participantCamera.gameObject.SetActive(false);
        Debug.LogWarning($"{participantCamera.name} closed camera");
    }

    private void ActivateMyLocalParticipantCameraAndMic()
    {
        var myVideoCamera = GetCamera(_videoCallHandler.myLocalParticipantIdentity);
        if (myVideoCamera == null) return;
        myVideoCamera.myUsername.text = _videoCallHandler.myLocalParticipantIdentity;
        ActivateIncomingCamera(myVideoCamera);
        _videoCallHandler._currentRoom.LocalParticipant.SetMicrophoneEnabled(true);
        playerUI.OpenVideoConferenceCameraUI();
    }

    private void DeActivateMyLocalParticipantCameraAndMic()
    {
        _videoCallHandler._currentRoom?.LocalParticipant.SetMicrophoneEnabled(false);
    }

    public void TriggerMyCamera()
    {
        if (!Object.HasInputAuthority || networkPlayer.isInitialized == false)
            return;

        _videoCallHandler.DisconnectFromCurrentRoom();
        //GetTokens
        GetMyToken(networkPlayer.currentRoomId.ToString(), networkPlayer.NickName.ToString());
        Debug.LogWarning($"Odaya Numarası {networkPlayer.currentRoomId} girenin nicki {networkPlayer.NickName}");
    }

    public void GetMyToken(string roomId, string nickName)
    {
        Singleton.Main.NetworkManager.GetRoomToken(roomId,
            nickName, GetRoomTokenCallbackForVideo);
    }

    private void GetRoomTokenCallbackForVideo(string data)
    {
        var json = JsonUtility.FromJson<RoomToken>(data);
        VideoCallHandler.RoomToken = json.token;
        _videoCallHandler.StartVideoConferenceForAutomatic();
    }

    #endregion
}