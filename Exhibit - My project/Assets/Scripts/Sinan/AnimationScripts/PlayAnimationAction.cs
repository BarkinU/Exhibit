using UnityEngine;
using System;
using Fusion;
using UnityEngine.InputSystem;


public class PlayAnimationAction : NetworkBehaviour
{
    [Serializable]
    public enum AnimationName
    {
        //Character Anims
        ANIM_HOUSE_DANCING,
        ANIM_SWING_DANCING,
        ANIM_TUT_HIP_HOP_DANCE,
        ANIM_CHARGE,
        ANIM_CLAPPING,
        ANIM_EXCITED,
        ANIM_SAD_IDLE,
        ANIM_TERRIFIED,
        ANIM_WHATEVER_GESTURE,
        ANIM_QUICK_FORMAL_BOW,
        ANIM_SALUTE,
        ANIM_SHAKING_HANDS,
        ANIM_STANDING_GREETING,
        ANIM_IDLE,
        ANIM_RUNNING,
        ANIM_WALKING,
        ANIM_SITTING
    }


    public Animator _animator;

    private int _lastVisibleJump;


    private AnimationDecision _currentAnimationDecision;


    private void Start()
    {
        currentAnimationName = "empty";
    }


    public string currentAnimationName { get; set; }

    public void ChangeAnimationTypeAndActivate(AnimationDecision animationDecision)
    {
        string animationString = GetAnimationString(animationDecision.AnimationName);
        currentAnimationName = animationString;
        RPC_TriggerAnimInputToAll(currentAnimationName);
    }

    public void ChangeAnimBool(AnimationDecision animationDecision, bool value)
    {
        string animationString = GetAnimationString(animationDecision.AnimationName);
        currentAnimationName = animationString;
        RPC_ChangeBoolAnimInputToAll(currentAnimationName, value);
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_TriggerAnimInputToAll(string animationName)
    {
        _animator.SetTrigger(animationName);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_ChangeBoolAnimInputToAll(string animationName, bool value)
    {
        _animator.SetBool(animationName, value);
    }


    private string GetAnimationString(AnimationName animationName)
    {
        switch (animationName)
        {
            case AnimationName.ANIM_HOUSE_DANCING:
                return CharacterAnimationStrings.ANIM_HOUSE_DANCING;
            case AnimationName.ANIM_SWING_DANCING:
                return CharacterAnimationStrings.ANIM_SWING_DANCING;
            case AnimationName.ANIM_TUT_HIP_HOP_DANCE:
                return CharacterAnimationStrings.ANIM_TUT_HIP_HOP_DANCE;
            case AnimationName.ANIM_CHARGE:
                return CharacterAnimationStrings.ANIM_CHARGE;
            case AnimationName.ANIM_CLAPPING:
                return CharacterAnimationStrings.ANIM_CLAPPING;
            case AnimationName.ANIM_SAD_IDLE:
                return CharacterAnimationStrings.ANIM_SAD_IDLE;
            case AnimationName.ANIM_TERRIFIED:
                return CharacterAnimationStrings.ANIM_TERRIFIED;
            case AnimationName.ANIM_WHATEVER_GESTURE:
                return CharacterAnimationStrings.ANIM_WHATEVER_GESTURE;
            case AnimationName.ANIM_QUICK_FORMAL_BOW:
                return CharacterAnimationStrings.ANIM_QUICK_FORMAL_BOW;
            case AnimationName.ANIM_SALUTE:
                return CharacterAnimationStrings.ANIM_SALUTE;
            case AnimationName.ANIM_SHAKING_HANDS:
                return CharacterAnimationStrings.ANIM_SHAKING_HANDS;
            case AnimationName.ANIM_STANDING_GREETING:
                return CharacterAnimationStrings.ANIM_STANDING_GREETING;
            case AnimationName.ANIM_IDLE:
                return CharacterAnimationStrings.ANIM_IDLE;
            case AnimationName.ANIM_RUNNING:
                return CharacterAnimationStrings.ANIM_RUNNING;
            case AnimationName.ANIM_WALKING:
                return CharacterAnimationStrings.ANIM_WALKING;
            case AnimationName.ANIM_EXCITED:
                return CharacterAnimationStrings.ANIM_EXCITED;
            case AnimationName.ANIM_SITTING:
                return CharacterAnimationStrings.ANIM_SITTING;
            default:
                throw new ArgumentOutOfRangeException(nameof(animationName), animationName, null);
        }
    }


    // public static void OnJumpChanged(Changed<PlayAnimationAction> changed)
    // {
    //     changed.LoadOld();
    //     int previousJumpCount = changed.Behaviour._jumpCount;
    //
    //     changed.LoadNew();
    //
    //     if (changed.Behaviour._jumpCount > previousJumpCount)
    //     {
    //         changed.LoadOld();
    //         changed.Behaviour._animator.SetTrigger(changed.Behaviour.currentAnimationName);
    //         // Play jump sound/particle effect
    //     }
    // }
}