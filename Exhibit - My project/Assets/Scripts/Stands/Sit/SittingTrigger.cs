using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;


public class SittingTrigger : NetworkBehaviour
{
    [SerializeField] private TableHandler tableRef;

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
            if (other.GetComponent<CharacterSittingHandler>().isChairAreaEntered == true)
            {
                return;
            }

            for (int i = 0; i < tableRef.chairsTransforms.Count; i++)
            {
                if (other.GetComponent<CharacterSittingHandler>().chairsTransforms.Count <
                    tableRef.chairsTransforms.Count)
                {
                    other.GetComponent<CharacterSittingHandler>().chairsTransforms
                        .Add(tableRef.chairsTransforms[i]);
                }
                else
                {
                    other.GetComponent<CharacterSittingHandler>().chairsTransforms[i] =
                        tableRef.chairsTransforms[i];
                }
            }

            other.GetComponent<CharacterSittingHandler>().isChairAreaEntered = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (Object.HasStateAuthority == false)
            return;

        if (other.CompareTag("Player"))
        {
            var otherComp = other.GetComponent<CharacterSittingHandler>();
            if (otherComp.isChairAreaEntered == false)
            {
                return;
            }

            otherComp.playAnimationAction.ChangeAnimBool(otherComp.animationDecision, false);
            otherComp.chairsTransforms.Clear();
            otherComp.isChairAreaEntered = false;
        }
    }
}