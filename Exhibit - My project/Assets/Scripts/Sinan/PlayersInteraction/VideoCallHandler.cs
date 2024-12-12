using System;
using System.Collections;
using UnityEngine;
using LiveKit;
using Fusion;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class VideoCallHandler : NetworkBehaviour
{
    public static string LivekitURL { get; set; }
    public static string RoomToken { get; set; }
    private LocalVideoTrack m_PreviewTrack;
    [Header("Player UI")] [SerializeField] private PlayerUI playerUI;

    [Header("Video Conference")]
    private Dictionary<TrackPublication, RawImage> m_Videos = new Dictionary<TrackPublication, RawImage>();

    private NetworkPlayer _networkPlayer;
    private InteractionHandler _interactionHandler;
    private GetRequestHandler _getRequestHandler;
    public Room _currentRoom;
    public GameObject viewParent;
    public GameObject panelParent;
    public bool isFullScreenOpen;
    private int rpcCount;
    [Header("Live Kit Variables")] public bool _videoCallInvitedOrCreated;
    public Dictionary<string, ParticipantCamera> _participantCameras = new Dictionary<string, ParticipantCamera>();
    [Networked] public string myLocalParticipantIdentity { get; set; }


    [Networked(OnChanged = nameof(OnVideoCallActivateChanged))]
    public bool isVideoCallActive { get; set; }

    private void Awake()
    {
        _interactionHandler = GetComponent<InteractionHandler>();
        _networkPlayer = GetComponent<NetworkPlayer>();
        _getRequestHandler = GetComponent<GetRequestHandler>();
    }

    public void Start()
    {
        LivekitURL = "wss://minego.livekit.cloud";
        ConnectListenersToFunction();
    }


    // ReSharper disable Unity.PerformanceAnalysis//
    public void StartVideoConferenceForClickInvite()
    {
        playerUI.OpenVideoConferenceCameraUI();
        _networkPlayer.Rpc_ChangePlayerStateFromStateAuthority(PlayerState.InVideoCall);

        ConnectListenersToFunction();

        //Trigger the senders video call
        if (_videoCallInvitedOrCreated)
        {
            TriggerTargetPlayerVideoConference();
        }

        StartCoroutine(StartVideoConferenceCO());
        ResetVideoCallValues();
    }

    public void StartVideoConferenceForAutomatic()
    {
        playerUI.OpenVideoConferenceCameraUI();
        StartCoroutine(StartVideoConferenceCO());
    }

    private void ConnectListenersToFunction()
    {
        playerUI.connectVideoCallButton.onClick.AddListener(() =>
        {
            if (string.IsNullOrWhiteSpace(RoomToken))
                return;

            m_PreviewTrack?.Detach();
            m_PreviewTrack?.Stop();
        });

        playerUI.disconnectButton.onClick.AddListener(DisconnectListener);
    }

    public void TriggerTargetPlayerVideoConference()
    {
        _interactionHandler.Rpc_ChangeVideoCallResponse(_getRequestHandler.tempTargetPlayer);
    }

    public void TriggerJoinedTargetPlayerVideoConference()
    {
        _interactionHandler.Rpc_JoinToVideoCallResponse(_getRequestHandler.tempTargetPlayer,
            _networkPlayer.currentRoomId);
    }

    private void ResetVideoCallValues()
    {
        _videoCallInvitedOrCreated = false;
    }


    //VideoCall
    IEnumerator StartVideoConferenceCO()
    {
        // New Room must be called when WebGL assembly is loaded
        _currentRoom = new Room();

        // Setup the callbacks before connecting to the Room
        _currentRoom.ParticipantConnected += (p) => { Debug.Log($"Participant connected: {p.Sid}"); };


        _currentRoom.LocalTrackPublished +=
            (publication, participant) => HandleAddedTrack(publication.Track, publication, participant);
        _currentRoom.LocalTrackUnpublished +=
            (publication, participant) => HandleRemovedTrack(publication.Track, publication);


        _currentRoom.TrackSubscribed += (track, publication, participant) =>
            HandleAddedTrack(track, publication, participant);
        _currentRoom.TrackUnsubscribed += (track, publication, participant) => HandleRemovedTrack(track, publication);

        var c = _currentRoom.Connect(LivekitURL, RoomToken);
        yield return c;

        if (c.IsError)
        {
            Debug.Log("Failed to connect to the room !");
            yield break;
        }

        Debug.Log("Connected to the room");
        myLocalParticipantIdentity = _currentRoom.LocalParticipant.Identity;
        RPC_ChangeMyLocalIdentityFromState(myLocalParticipantIdentity);
        yield return _currentRoom.LocalParticipant.EnableCameraAndMicrophone();
    }


    private void HandleAddedTrack(Track track, TrackPublication publication, Participant participant)
    {
        if (track.Kind == TrackKind.Video)
        {
            if (playerUI.ViewContainer.transform.childCount >= 6)
                return; // No space to show more than 6 tracks

            var video = track.Attach() as HTMLVideoElement;
            var newView = Instantiate(playerUI.ViewPrefab, playerUI.ViewContainer.transform);
            m_Videos.Add(publication, newView);
            newView.GetComponent<ParticipantCamera>().videoCallHandler = this;
            newView.GetComponent<ParticipantCamera>().myUsername.text = participant.Identity;
            video.VideoReceived += tex => { newView.texture = tex; };

            if (_participantCameras.ContainsKey(participant.Identity))
            {
                _participantCameras[participant.Identity] = newView.GetComponent<ParticipantCamera>();
            }
            else
            {
                _participantCameras.Add(participant.Identity, newView.GetComponent<ParticipantCamera>());
            }


            if (participant.Identity != _currentRoom.LocalParticipant.Identity)
            {
                newView.gameObject.SetActive(false);
            }
        }
        else if (track.Kind == TrackKind.Audio && publication is RemoteTrackPublication)
        {
            track.Attach();
        }
    }

    private void HandleRemovedTrack(Track track, TrackPublication publication)
    {
        track.Detach();

        if (m_Videos.TryGetValue(publication, out var view))
        {
            // _participantCameras.Remove(view.GetComponent<ParticipantCamera>().myUsername.text);
            // view.gameObject.SetActive(true);
            Destroy(view.gameObject);
        }
    }

    public void OpenCloseScreenShare()
    {
        if (_networkPlayer.playerCurrentState == PlayerState.InVideoCall &&
            _interactionHandler.GetCamera(myLocalParticipantIdentity).gameObject.activeSelf && _currentRoom != null)
            _currentRoom.LocalParticipant.SetScreenShareEnabled(!_currentRoom.LocalParticipant.IsScreenShareEnabled);
    }

    public void OpenCloseCameraButtonListener()
    {
        if (_networkPlayer.playerCurrentState == PlayerState.InVideoCall &&
            _interactionHandler.GetCamera(myLocalParticipantIdentity).gameObject.activeSelf && _currentRoom != null)
        {
            _currentRoom.LocalParticipant.SetCameraEnabled(!_currentRoom.LocalParticipant.IsCameraEnabled);
            _interactionHandler.GetCamera(myLocalParticipantIdentity).ChangeMyTextureToClosed();
        }
    }

    public void OpenCloseMicButtonListener()
    {
        if (_networkPlayer.playerCurrentState == PlayerState.InVideoCall &&
            _interactionHandler.GetCamera(myLocalParticipantIdentity).gameObject.activeSelf && _currentRoom != null)
            _currentRoom.LocalParticipant.SetMicrophoneEnabled(!_currentRoom.LocalParticipant.IsMicrophoneEnabled);
    }


    static void OnVideoCallActivateChanged(Changed<VideoCallHandler> changed)
    {
        bool currentVideoCallState = changed.Behaviour.isVideoCallActive;

        if (currentVideoCallState)
        {
            changed.Behaviour.StartVideoConferenceForClickInvite();
            changed.Behaviour._networkPlayer.Rpc_AddMyPlayerRefToServerList(
                changed.Behaviour._networkPlayer.currentRoomId, changed.Behaviour.Object.InputAuthority);
            Debug.LogWarning(
                $"Player Joined to room {changed.Behaviour._networkPlayer.currentRoomId} playerAdded to the party");
        }
    }

    public void DisconnectFromCurrentRoom()
    {
        _currentRoom?.Disconnect();
        _participantCameras.Clear();
    }

    private void DisconnectListener()
    {
        DisconnectFromCurrentRoom();
        playerUI.CloseVideoConferenceCameraUI();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ChangeMyLocalIdentityFromState(string myParticipantIdentity, RpcInfo info = default)
    {
        AddRemoteParticipantToList(myParticipantIdentity);
    }

    private void AddRemoteParticipantToList(string myParticipantIdentity)
    {
        myLocalParticipantIdentity = myParticipantIdentity;
    }
}