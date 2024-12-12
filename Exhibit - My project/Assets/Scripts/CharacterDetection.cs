using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;

public class CharacterDetection : NetworkBehaviour
{
    [Header("Detect Variables")] private Collider[] overlapResults = new Collider[20];
    private string[] overlapResultsParticipant = new string[20];
    [SerializeField] private float characterDetectionRange = 5f;
    [SerializeField] private List<string> overlapResultsTempList = new List<string>();
    public List<string> charactersInRangeOldList = new List<string>();
    [SerializeField] private LayerMask layerMask;

    [Header("Other Components")] [SerializeField]
    private InteractionHandler _interactionHandler;

    private VideoCallHandler _videoCallHandler;
    private bool _isInitialized = false;
    private int _number = 0;


    private void Start()
    {
        _interactionHandler = GetComponent<InteractionHandler>();
        _videoCallHandler = GetComponent<VideoCallHandler>();
        _isInitialized = true;
    }


    public override void FixedUpdateNetwork()
    {
        if (_isInitialized)
            CheckMyRange();
    }

    private void CheckMyRange()
    {
        if (!Object.HasInputAuthority)
            return;
        Array.Clear(overlapResults, 0, overlapResults.Length);
        Array.Clear(overlapResultsParticipant, 0, overlapResultsParticipant.Length);
        overlapResultsTempList.Clear();
        _number = Physics.OverlapSphereNonAlloc(transform.position, characterDetectionRange, overlapResults, layerMask);

        if (_number == 0)
        {
            Debug.LogWarning($"number = {_number}");
            return;
        }

        //Add All Participant Identity to List
        for (int i = 0; i < _number; i++)
        {
            overlapResultsParticipant[i] =
                overlapResults[i].GetComponent<VideoCallHandler>().myLocalParticipantIdentity;
        }

        overlapResultsTempList = overlapResultsParticipant.ToList();

        //Remove My Identity
        if (!string.IsNullOrEmpty(_videoCallHandler.myLocalParticipantIdentity))
        {
            overlapResultsTempList.Remove(_videoCallHandler.myLocalParticipantIdentity);
        }

        //Draw Debug Between Characters
        for (int i = 0; i < overlapResultsTempList.Count; i++)
        {
            if (string.IsNullOrEmpty(overlapResultsTempList[i]))
                continue;

            Debug.DrawLine(transform.position, overlapResults[i].transform.position, Color.red);
        }

        //OnTrigger Enter
        for (int i = 0; i < overlapResultsTempList.Count; i++)
        {
            if (string.IsNullOrEmpty(overlapResultsTempList[i]) ||
                charactersInRangeOldList.Contains(overlapResultsTempList[i]) ||
                overlapResultsTempList[i] == _videoCallHandler.myLocalParticipantIdentity)
            {
                continue;
            }

            charactersInRangeOldList.Add(overlapResultsTempList[i]);
            WhenCharacterCollide(overlapResultsTempList[i]);
            Debug.LogWarning($"Character Enter: {overlapResultsTempList[i]}");
        }


        //OnTrigger Exit
        for (int i = 0; i < charactersInRangeOldList.Count; i++)
        {
            if (string.IsNullOrEmpty(charactersInRangeOldList[i]) ||
                overlapResultsTempList.Contains(charactersInRangeOldList[i]))
            {
                continue;
            }

            Debug.LogWarning($"Character Exit: {charactersInRangeOldList[i]}");
            WhenCharacterExit(charactersInRangeOldList[i]);
        }
    }


    private void WhenCharacterCollide(string participantIdentity)
    {
        Debug.LogWarning($"Camera Added: {participantIdentity}");
        _interactionHandler.CheckMyParticipantListAndActivate(participantIdentity);
    }

    private void WhenCharacterExit(string participantIdentity)
    {
        Debug.LogWarning($"Character removed: {participantIdentity}");
        _interactionHandler.CheckMyParticipantListAndDeActivate(participantIdentity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, characterDetectionRange);
    }
}