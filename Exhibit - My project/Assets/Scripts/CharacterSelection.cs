using System.Collections.Generic;
using Fusion;
using UnityEditor.Rendering;
using UnityEngine;

public class CharacterSelection : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnSelection))]
    private int SelectionIndex { get; set; }

    [SerializeField] private List<NetworkObject> models;

    public void Send(int trial)
    {
        RPCCall(trial);
    }

    public static void OnSelection(Changed<CharacterSelection> changed)
    {
        changed.Behaviour.Select();
    }

    public void Select()
    {
        for (int i = 0; i < models.Count; i++)
        {
            models[i].gameObject.SetActive(false);
        }

        models[SelectionIndex].gameObject.SetActive(true);
    }


    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPCCall(int index)
    {
        SelectionIndex = index;
    }
}