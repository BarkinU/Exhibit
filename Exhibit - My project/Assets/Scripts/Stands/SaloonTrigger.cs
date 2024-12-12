using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;


public class SaloonTrigger : NetworkBehaviour
{
    [SerializeField] private SaloonVisualsHandler triggerSaloon;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
        {
            GetComponent<Collider>().enabled = false;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (Object.HasStateAuthority == false)
            return;
        
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<NetworkPlayer>().currentRoomId == triggerSaloon.roomMeetingIds)
            {
                Debug.LogWarning($"girmedim salona {triggerSaloon.roomMeetingIds}");
                return;
            }

            other.GetComponent<NetworkPlayer>().ChangeCurrentRoomID(triggerSaloon.roomMeetingIds);
            other.GetComponent<NetworkPlayer>().ChangePlayerState(PlayerState.InVideoCall);
            Debug.LogWarning($"onchanged calıştı");
        }
    }
}