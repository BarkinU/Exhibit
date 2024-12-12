using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion.Sample.DedicatedServer;

public class SessionListUIHandler : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public GameObject sessionItemListPrefab;
    public VerticalLayoutGroup verticalLayoutGroup;

    private void Awake()
    {
        ClearList();
    }

    public void ClearList()
    {
        //Delete all children of the vertical layout group
        foreach (Transform child in verticalLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        //Hide the status message
        statusText.gameObject.SetActive(false);
    }

    public void AddToList(SessionInfo sessionInfo)
    {
        //Add a new item to the list
        SessionInfoListUIItem addedSessionInfoListUIItem =
            Instantiate(sessionItemListPrefab, verticalLayoutGroup.transform).GetComponent<SessionInfoListUIItem>();

        addedSessionInfoListUIItem.SetInformation(sessionInfo);

        //Hook up events
        addedSessionInfoListUIItem.OnJoinSession += AddedSessionInfoListUIItem_OnJoinSession;
    }

    private void AddedSessionInfoListUIItem_OnJoinSession(SessionInfo sessionInfo)
    {
        MainMenuUIHandler mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>();
        mainMenuUIHandler._sessionName = sessionInfo.Name;
        mainMenuUIHandler.StartClient();
        mainMenuUIHandler.OnJoiningServer();
    }

    public void OnNoSessionsFound()
    {
        ClearList();

        statusText.text = "No game session found";
        statusText.gameObject.SetActive(true);
    }

    public void OnLookingForGameSessions()
    {
        ClearList();

        statusText.text = "Looking for game sessions";
        statusText.gameObject.SetActive(true);
    }
}