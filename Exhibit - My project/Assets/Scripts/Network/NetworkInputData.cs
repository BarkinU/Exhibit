using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 MovementInput;
    public Vector3 CameraRotation;
    public NetworkBool IsJumpPressed;
    public Vector3 AimForwardVector;
    public NetworkBool IsDartButtonPressed;
    public NetworkBool IsBallShootButtonPressed;
    public bool Sprinting;
    public float RadToDeg;
    public NetworkBool IsCharacterSelectButtonPressed;
    public NetworkBool IsAnimationButtonPressed;
    public NetworkBool IsThereAnyRequest;
    public NetworkBool isHoverboardActive;
    public NetworkBool isSitButtonPressed;
}