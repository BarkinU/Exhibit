using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerPlaybackHandler : NetworkBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    [Networked(OnChanged = nameof(SwitchState))]
    public NetworkBool videoTrigger { get; set; }

    private void Start()
    {
        InvokeRepeating(nameof(CheckIfVideoLoaded), 0f, 1f);

    }

    private void CheckIfVideoLoaded()
    {
        if (videoPlayer.isPrepared)
        {
            videoPlayer.Pause();
            CancelInvoke();
        }
        Debug.Log("Video is not loaded");
    }

    public static void SwitchState(Changed<VideoPlayerPlaybackHandler> changed)
    {

        var behaviour = changed.Behaviour;
        if (behaviour.videoPlayer.isPlaying)
        {
            Debug.Log("Pausing video");
            behaviour.videoPlayer.Pause();
        }
        else
        {
            Debug.Log("Playing video");
            behaviour.videoPlayer.Play();
        }
    }

}
