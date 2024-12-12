using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class CharacterSittingHandler : NetworkBehaviour
{
    [Header("Global Message")] public PlayerUI playerUI;

    [Networked(OnChanged = nameof(OnEnterChairArea))]
    public bool isChairAreaEntered { get; set; }

    public List<Transform> chairsTransforms = new List<Transform>();
    private NetworkTransform myTransform;
    [Networked] private Vector3 myOldTransform { get; set; }
    [Networked] public bool IsSitButtonPressed { get; set; }
    public AnimationDecision animationDecision;
    public PlayAnimationAction playAnimationAction;


    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData networkInputData)) return;

        if (!networkInputData.isSitButtonPressed) return;
        Debug.Log($"Chair Sit Started1");

        if (!isChairAreaEntered) return;
        Debug.Log($"Chair Sit Started3");

        if (!IsSitButtonPressed)
        {
            SitOnChair();
        }
        else
        {
            StandFromChair();
        }
    }

    private void Awake()
    {
        myTransform = GetComponent<NetworkTransform>();
        playerUI = GetComponentInChildren<PlayerUI>();
    }

    static void OnEnterChairArea(Changed<CharacterSittingHandler> changed)
    {
        var isInArea = changed.Behaviour.isChairAreaEntered;

        switch (isInArea)
        {
            case true:
                changed.Behaviour.SitPanelOpenerCloser(true);
                break;
            case false:
                changed.Behaviour.SitPanelOpenerCloser(false);
                break;
        }
    }

    private void SitOnChair()
    {
        //Teleport to chair position
        if (Object.HasStateAuthority)
        {
            IsSitButtonPressed = true;
            myOldTransform = transform.position;

            transform.rotation = GetClosestEnemy().rotation;

            myTransform.TeleportToPosition(GetClosestEnemy().position);
        }

        if (Object.HasInputAuthority)
        {
            animationDecision.AnimationName = PlayAnimationAction.AnimationName.ANIM_SITTING;
            playAnimationAction.ChangeAnimBool(animationDecision, true);
        }
    }

    private void StandFromChair()
    {
        if (Object.HasStateAuthority)
        {
            IsSitButtonPressed = false;
            myTransform.TeleportToPosition(myOldTransform); //Teleport to chair position
        }

        if (Object.HasInputAuthority)
        {
            animationDecision.AnimationName = PlayAnimationAction.AnimationName.ANIM_SITTING;
            playAnimationAction.ChangeAnimBool(animationDecision, false);
        }
    }

    private void SitPanelOpenerCloser(bool isOpen)
    {
        if (!Object.HasInputAuthority)
            return;

        playerUI.wantToSitPanel.SetActive(isOpen);
    }

    private Transform GetClosestEnemy()
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        for (int i = 0; i < chairsTransforms.Count; i++)
        {
            Vector3 directionToTarget = chairsTransforms[i].position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = chairsTransforms[i];
            }
        }

        return bestTarget;
    }
}