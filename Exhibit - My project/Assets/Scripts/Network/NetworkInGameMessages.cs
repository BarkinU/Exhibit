using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkInGameMessages : NetworkBehaviour
{
    InGameMessagesUIHandler inGameMessagesUIHandler;

    public void SendInGameRPCMessage(string userNickname, string message, ChatState chatType)
    {
        RPC_InGameGlobalMessage(userNickname, message, chatType);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_InGameGlobalMessage(string nickname, string message, ChatState chatType, RpcInfo info = default)
    {
        CheckOnMessage(nickname, message, chatType);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SendGlobalMessageInputToStateAuthority(string nickname, string message, ChatState chatType,
        RpcInfo info = default)
    {
        SendInGameRPCMessage(nickname, message, chatType);
    }

    // Open Selected Character UI for Video Conference
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.All)]
    public void Rpc_SendGroupMessageToTargetPlayers([RpcTarget] PlayerRef player, string nickname, string message,
        ChatState chatType, RpcInfo info = default)
    {
        CheckOnMessage(nickname, message, chatType);
    }

    private void CheckOnMessage(string nickname, string message,
        ChatState chatType)
    {
        if (Object.HasStateAuthority)
            return;
        if (inGameMessagesUIHandler == null)
        {
            inGameMessagesUIHandler =
                NetworkPlayer.Local.playerUI.GetComponentInChildren<InGameMessagesUIHandler>();
        }

        if (inGameMessagesUIHandler != null)
        {
            inGameMessagesUIHandler.OnGameMessageReceived(nickname, message, chatType);
        }
    }
}