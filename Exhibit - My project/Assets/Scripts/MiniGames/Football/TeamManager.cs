using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class TeamManager : NetworkBehaviour
{

    [Networked]
    [Capacity(2)] // Sets the fixed capacity of the collection
    NetworkArray<NetworkString<_32>> Team1 { get; } =
        MakeInitializer(new NetworkString<_32>[] { "#0", "#1", "#2", "#3" });

    public override void FixedUpdateNetwork()
    {

    }


}
