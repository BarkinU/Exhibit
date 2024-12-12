using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sample.DedicatedServer;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

public enum PlayerState
{
    //Character States
    InVideoCall,
    InStand,
    InPrivateCall,
    Idle
}


public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public struct Party : INetworkStruct
    {
        //public List<PlayerRef> playersInParty { get; set; }
        [Networked] [Capacity(6)] public NetworkLinkedList<PlayerRef> playersInParty { get; }
    }

    public TextMeshProUGUI playerNickNameTM;
    public Transform playerModel;

    public GameObject[] localCameraHandler;
    public GameObject localUI;


    [SerializeField] private GameObject cameraController;


    private bool _isPublicJoinMessageSent;

    public static NetworkPlayer Local { get; set; }

    [Networked(OnChanged = nameof(OnNickNameChanged))]
    public NetworkString<_16> NickName { get; set; }

    public ServerPlayerManager serverPlayerManager;
    public InteractionHandler interactionHandler;


    // Sinan Variables


    [Networked] public PlayerRef myTargetSenderOfRequest { get; set; }

    [Networked] public PlayerState playerCurrentState { get; set; }

    [Networked(OnChanged = nameof(OnEnteredRoom))]
    public int currentRoomId { get; set; }

   
    public bool isInitialized;
    private const int hallConstantRoomId = 100;

    [Header("Global Message")] public PlayerUI playerUI;

    [Networked] public Party myParty { get; set; }

    private void Awake()
    {
        interactionHandler = GetComponent<InteractionHandler>();
    }

    private void Start()
    {
        //Hall RoomId
        currentRoomId = hallConstantRoomId;
        playerCurrentState = PlayerState.InVideoCall;

        isInitialized = true;
    }

    

    private void OnDestroy()
    {
        if (localCameraHandler[0] != null && localCameraHandler[1] != null)
        {
            Destroy(localCameraHandler[0]);
            Destroy(localCameraHandler[1]);
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (!Runner.IsServer) return;

        if (Runner.TryGetPlayerObject(player, out NetworkObject playerLeftNetworkObject))
        {
            if (playerLeftNetworkObject == Object)
                Local.GetComponent<NetworkInGameMessages>().SendInGameRPCMessage(
                    playerLeftNetworkObject.GetComponent<NetworkPlayer>().NickName.ToString(), "left",
                    ChatState.Global);
            Debug.Log($"{Time.time} Despawned player {playerLeftNetworkObject.name}");
            Runner.Despawn(playerLeftNetworkObject);
        }
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            //Sets the layer of the local players model
            Utils.SetRenderLayerInChildren(playerModel, LayerMask.NameToLayer(LayerMasks.LocalPlayerModel));

            //Enable the local camera
            localCameraHandler[0].gameObject.SetActive(true);
            cameraController.transform.parent = null;
            //Enable UI for local player
            localUI.SetActive(true);
            if (string.IsNullOrEmpty(Singleton.Main.gameManager.PlayerNickName))
            {
                var testName = "User: " + UnityEngine.Random.Range(1, 10000);
                RPC_SetNickName(testName);
            }
            else
            {
                RPC_SetNickName(Singleton.Main.gameManager.PlayerNickName);
            }

            Debug.Log("Spawned local player");
        }
        else
        {
            localCameraHandler[0].gameObject.SetActive(false);
            localCameraHandler[1].gameObject.SetActive(false);
            cameraController.SetActive(false);
            this.enabled = false;
            //Disable UI for remote player
            localUI.SetActive(false);

            Debug.Log($"{Time.time} Spawned remote player");
        }

        //Set the Player as a player object
        Runner.SetPlayerObject(Object.InputAuthority, Object);

        //Make it easier to tell which player is which.
        transform.name = $"P_{Object.Id}";
    }

    static void OnNickNameChanged(Changed<NetworkPlayer> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.NickName}");

        changed.Behaviour.OnNickNameChanged();
    }

    private void OnNickNameChanged()
    {
        Debug.Log($"Nickname changed for player to {NickName} for player {gameObject.name}");

        playerNickNameTM.text = NickName.ToString();
    }

    static void OnEnteredRoom(Changed<NetworkPlayer> changed)
    {
        var newRoomId = changed.Behaviour.currentRoomId;

        changed.LoadOld();

        var oldRoomId = changed.Behaviour.currentRoomId;

        changed.LoadNew();

        if (newRoomId != oldRoomId)
        {
            changed.Behaviour.interactionHandler.TriggerMyCamera();
        }
    }

    

   

   

    #region SendRequestWithRPCsToServer

    public void ChangePlayerState(PlayerState playerState)
    {
        playerCurrentState = playerState;
    }

    public void ChangeCurrentRoomID(int roomID)
    {
        currentRoomId = roomID;
    }

    public void ChangeSenderOfRequest(PlayerRef playerRef)
    {
        myTargetSenderOfRequest = playerRef;
    }

    #endregion

    #region RPCs

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default)
    {
        if (!string.IsNullOrEmpty(nickName))
        {
            Debug.Log($"[RPC] SetNickName {nickName}");
            this.NickName = nickName;
        }
    }

    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void Rpc_SetMyRoomIdFromStateAuthority(int roomId)
    {
        ChangeCurrentRoomID(roomId);
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_ChangePlayerStateFromStateAuthority(PlayerState playerNewState)
    {
        ChangePlayerState(playerNewState);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_ChangeMyTargetFromStateAuthority(PlayerRef setTarget)
    {
        ChangeSenderOfRequest(setTarget);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_AddMyPlayerRefToServerList(int roomId, PlayerRef playerRef)
    {
        GetComponent<NetworkPlayer>().serverPlayerManager.PlayerJoinedToRoom(roomId, playerRef);
    }

    #endregion
}