using Fusion;
using UnityEngine;
using UnityEngine.Video;

public class CollisionDetection : NetworkBehaviour
{
    private DartFire _dartFire;
    [SerializeField] private PlayerInputs inputs;
    private BarkinThirdPersonController controller;

    private void Start()
    {
        controller = GetComponent<BarkinThirdPersonController>();
        _dartFire = GetComponent<DartFire>();
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            this.enabled = true;
        }
        else
        {
            this.enabled = false;
        }

    }
    public void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority)
            return;

        // if the player is in dart game
        if (other.CompareTag("DartArea"))
        {
            _dartFire.IsCharacterInDartGame = true;
        }

        // if the player is in video player range then play the video
        if (other.CompareTag("VideoPlayer"))
        {
            Debug.Log("Video player trigger");
            other.GetComponent<VideoPlayerPlaybackHandler>().videoTrigger = true;

        }

        // If the player is in the elevator 
        if (other.CompareTag("Elevator") && !controller.isHoverboardActive)
        {
            controller.IsCharacterInElevator = true;
            // make a notification to the player that he cannot use elevator while hoverboard is active
            controller.CanGravityAct = false;
        }

        if (other.CompareTag("ConferenceEntrance"))
        {
            controller.SendConferenceEntranceRPC();
        }

    }

    public void OnTriggerExit(Collider other)
    {
        if (!Object.HasStateAuthority)
            return;


        // if the player is out of dart game
        if (other.CompareTag("DartArea"))
        {
            _dartFire.IsCharacterInDartGame = false;
        }

        // set video player pause when player is out of range
        if (other.CompareTag("VideoPlayer"))
        {
            Debug.Log("Video player trigger");
            other.GetComponent<VideoPlayerPlaybackHandler>().videoTrigger = false;
        }

        // If the palyer is out of the elevator 
        if (other.CompareTag("Elevator") && !controller.isHoverboardActive)
        {
            controller.IsCharacterInElevator = false;
            controller.CanGravityAct = true;
        }
        if (other.CompareTag("ConferenceExit"))
        {
            controller.SendConferenceExitRPC();
        }

    }

}