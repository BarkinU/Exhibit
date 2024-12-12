using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InGameMessagesUIHandler : MonoBehaviour
{
    public Transform messageParent;
    public Message messagePrefab;
    private Message temporaryMessage;


    public void OnGameMessageReceived(string userNickname, string message, ChatState chatType)
    {
        CreateMessage(userNickname, message, chatType);
    }

    private void CreateMessage(string userNickname, string message, ChatState chatType)
    {
        temporaryMessage = Instantiate(messagePrefab, Vector3.zero, Quaternion.identity);
        temporaryMessage.transform.SetParent(messageParent);
        temporaryMessage.transform.localScale = Vector3.one;
        temporaryMessage.transform.localPosition = Vector3.zero;
        temporaryMessage.transform.localRotation = Quaternion.identity;
        temporaryMessage.SetMessageContent(userNickname, message, chatType);
    }
}