using Fusion;
using UnityEngine;
using UnityEngine.Video;

public class CollisionDetection2 : NetworkBehaviour
{
    private DartFire _dartFire;
    [SerializeField] private PlayerInputs inputs;
    private BarkinThirdPersonController controller;

    private void Start()
    {
        controller = GetComponent<BarkinThirdPersonController>();
        _dartFire = GetComponent<DartFire>();
        Invoke(nameof(GetPlayerInputs), 4f);
    }


    // this will be invoked because after a time the player will be spawned
    private void GetPlayerInputs()
    {
        inputs = GetComponentInChildren<PlayerInputs>();
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
            other.GetComponent<VideoPlayerPlaybackHandler>().videoTrigger = true;
        }

        // If the palyer is in the elevator 
        if (other.CompareTag("Elevator"))
        {
            controller.IsCharacterInElevator = true;
            controller.CanGravityAct = false;
        }
    }

    // public void OnTriggerStay(Collider other)
    // {
    //     if (!HasInputAuthority)
    //         return;
    //     if (other.CompareTag("Player"))
    //     {
    //         
    //     }
    // }


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
            other.GetComponent<VideoPlayerPlaybackHandler>().videoTrigger = false;
        }

        // If the palyer is out of the elevator 
        if (other.CompareTag("Elevator"))
        {
            controller.IsCharacterInElevator = false;
            controller.CanGravityAct = true;
        }
    }
}