using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;
using Fusion.Sockets;
using System;

public class Hoverboard : NetworkBehaviour
{
    private NetworkRigidbody networkRigidbody;

    private void Awake()
    {
        networkRigidbody = GetComponent<NetworkRigidbody>();
    }

    public float multiplier;
    public float moveForce, turnTorque;
    public Transform[] anchors = new Transform[4];
    RaycastHit[] hits = new RaycastHit[4];
    public INetworkRunnerCallbacks[] ali;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
        }
    }

    public override void FixedUpdateNetwork()
    {
        for (int i = 0; i < 4; i++)
        {
            ApplyForce(anchors[i], hits[i]);
        }
        if (GetInput(out NetworkInputData networkInputData))
        {
            networkRigidbody.Rigidbody.AddForce(networkInputData.MovementInput.x * moveForce * transform.forward);
            networkRigidbody.Rigidbody.AddTorque(networkInputData.MovementInput.y * turnTorque * transform.up);
        }
    }

    private void ApplyForce(Transform anchor, RaycastHit hit)
    {
        if (Physics.Raycast(anchor.position, -anchor.up, out hit))
        {
            float force = 0;
            force = Mathf.Abs(1 / (hit.point.y - anchor.position.y));
            networkRigidbody.Rigidbody.AddForceAtPosition(transform.up * force * multiplier, anchor.position, ForceMode.Acceleration);
        }
    }

}
