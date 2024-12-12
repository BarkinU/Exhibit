using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;

public class FootballBall : NetworkBehaviour
{
    [Networked] public TickTimer resetBallTimer { get; set; }
    [Networked] public TickTimer checkTimer { get; set; }

    [Networked] public Vector3 tempPosition { get; set; }
    NetworkRigidbody networkRigidbody;
    public NetworkString<_16> lastShotPlayer;
    public TMP_Text lastShotPlayerText;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        networkRigidbody = GetComponent<NetworkRigidbody>();


    }

    public override void FixedUpdateNetwork()
    {
        if (checkTimer.ExpiredOrNotRunning(Runner))
        {
            CheckIfBallIsSamePositionOverTime();
        }

    }
    private void CheckIfBallIsSamePositionOverTime()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        checkTimer = TickTimer.CreateFromSeconds(Runner, 1f);

        if (Vector3.Distance(transform.position, ballSpawnPoint.position) < 3f)
        {
            Debug.Log("Ball is in same position no need to reset");
            return;
        }

        if (Vector3.Distance(transform.position, tempPosition) > 1f)
        {
            Debug.Log("Ball is moving, reseting timer");
            resetBallTimer = TickTimer.CreateFromSeconds(Runner, 30f);
        }

        if (resetBallTimer.Expired(Runner))
        {
            ResetBall();
            Debug.Log("Ball is in same position for 30 seconds, reseting ball");
            Debug.Log("Ball is in same position for 30 seconds, reseting ball");


        }

        tempPosition = transform.position;
    }
    [SerializeField] private Transform ballSpawnPoint;
    [Networked(OnChanged = nameof(OnGoal))] public NetworkBool isGoal { get; set; }

    public void OnTriggerEnter(Collider other)
    {
        if (!Runner)
        {
            return;
        }
        if (!Object.HasStateAuthority)
            return;

        if (other.gameObject.CompareTag("GoalPost"))
        {
            isGoal = true;
            Invoke(nameof(ResetBall), 1f);
        }

        if (other.gameObject.CompareTag("FootballGameTrigger"))
        {
            Invoke(nameof(ResetBall), 1f);
        }
    }
    public MeshRenderer goalPostMaterial;
    public static void OnGoal(Changed<FootballBall> changed)
    {
        Debug.Log("changed.Behaviour.isGoal: " + changed.Behaviour.isGoal);
        changed.Behaviour.lastShotPlayerText.text = changed.Behaviour.lastShotPlayer + " scored!";
        changed.Behaviour.goalPostMaterial.material.color = changed.Behaviour.isGoal ? Color.red : Color.white;
    }



    private void ResetBall()
    {
        transform.position = ballSpawnPoint.position + Vector3.up * 2f;
        networkRigidbody.Rigidbody.velocity = Vector3.zero;
        networkRigidbody.Rigidbody.angularVelocity = Vector3.zero;
        isGoal = false;

    }
}
