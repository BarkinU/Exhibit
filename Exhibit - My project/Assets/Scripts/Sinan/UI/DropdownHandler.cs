using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public enum ChatState
{
    Global,
    Group,
    Normal
}

public class DropdownHandler : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown chatDropdown;
    private List<string> chatStateNames = new List<string> { "Global", "Group", "Normal" };
    [SerializeField] private PlayerUI playerUI;
    
    
    private void Start()
    {
        PopulateList();
    }

    void PopulateList()
    {
        chatDropdown.AddOptions(chatStateNames);
        playerUI.userChatMessageInputFieldText.color = Color.cyan;
    }

    public void DropdownIndexChanged(int index)
    {
        switch (index)
        {
            case 0:
                playerUI.userChatMessageInputFieldText.color = Color.cyan;
                playerUI.currentChatState = ChatState.Global;
                break;
            case 1:
                playerUI.userChatMessageInputFieldText.color = Color.green;
                playerUI.currentChatState = ChatState.Group;
                break;
            case 2:
                playerUI.userChatMessageInputFieldText.color = Color.yellow;
                playerUI.currentChatState = ChatState.Normal;
                break;
        }
    }

    private void DropdownChangedIndex(TMP_Dropdown dropdown)
    {
        int index = dropdown.value;
    }
}