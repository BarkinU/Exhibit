using UnityEngine;
using TMPro;

public class Message : MonoBehaviour
{
    public TMP_Text messageText;

    public void SetMessageContent(string userNickname, string incomingMessage, ChatState chatType)
    {
        messageText.text = $"<color=grey><b>{userNickname} : </b></color>{incomingMessage}";
        ChangeIncomingTextColor(chatType);
    }

    private void ChangeIncomingTextColor(ChatState chatType)
    {
        switch (chatType)
        {
            case ChatState.Global:
                messageText.color = Color.cyan;
                break;
            case ChatState.Group:
                messageText.color = Color.green;
                break;
            case ChatState.Normal:
                messageText.color = Color.yellow;
                break;
        }
    }
}