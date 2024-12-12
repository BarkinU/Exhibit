using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class Football : NetworkBehaviour
{
    private GameObject ball;
    private float force;
    private BarkinThirdPersonController controller;
    private NetworkBool isCoroutineActive { get; set; }


    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.IsBallShootButtonPressed)
            {
                if (ball)
                {
                    if (Vector3.Distance(transform.position, ball.transform.position) < 1f)
                    {
                        if (!isCoroutineActive)
                        {
                            controller.animator.SetTrigger("isBallShot");
                            StartCoroutine(Shoot());
                        }
                    }
                }
            }
        }
    }

    public override void Spawned()
    {
        force = 40f;
        controller = GetComponent<BarkinThirdPersonController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority)
            return;

        if (other.gameObject.CompareTag("FootballBall"))
        {
            ball = other.gameObject;
            other.GetComponent<NetworkRigidbody>().Rigidbody.AddForce(transform.forward * force * Mathf.Pow(controller.Controller.velocity.magnitude, 2f)
            , ForceMode.Acceleration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!Object.HasStateAuthority)
            return;

    }

    private IEnumerator Shoot()
    {

        if (Object.HasInputAuthority)
        {
            yield break;
        }

        isCoroutineActive = true;

        yield return new WaitForSeconds(0.25f);
        ball.GetComponent<NetworkRigidbody>().Rigidbody.AddForce(transform.forward * force + transform.up * force / Random.Range(4f, 12f), ForceMode.Impulse);
        ball.GetComponent<FootballBall>().lastShotPlayer = GetComponent<NetworkPlayer>().NickName;
        Debug.Log("Footbal ball shot");
        isCoroutineActive = false;
    }

}
